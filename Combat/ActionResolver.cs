using System;
using System.Collections.Generic;
using System.Linq;
using DemoDunjia.Data;
using DemoDunjia.Entities;
using DemoDunjia.Systems;

namespace DemoDunjia.Combat;

public sealed class ActionResolver
{
    private readonly BattleConfig _config;
    private readonly CombatLog _log;
    private readonly RouletteSystem _roulette;
    private readonly DefenseSystem _defense;
    private readonly StatusEffectSystem _status;
    private readonly ChargeSystem _charge;

    public ActionResolver(BattleConfig config, CombatLog log, RouletteSystem roulette, DefenseSystem defense, StatusEffectSystem status, ChargeSystem charge)
    {
        _config = config;
        _log = log;
        _roulette = roulette;
        _defense = defense;
        _status = status;
        _charge = charge;
    }

    public ActionResolutionResult ResolvePlayerAction(PlayerCombatActor player, MonsterCombatActor monster, CombatActionType playerAction, MonsterSkillData monsterSkill)
    {
        player.IsDefending = playerAction == CombatActionType.Defend;
        monster.IsDefending = monsterSkill.ActionType == CombatActionType.Defend;

        if (playerAction == CombatActionType.Defend)
        {
            player.GainShield(_config.BaseDefenseShieldGain);
            _log.Add($"玩家选择防御，先获得护盾 +{_config.BaseDefenseShieldGain}（当前护盾 {player.Shield}）");
        }

        var result = new ActionResolutionResult();
        if (playerAction == CombatActionType.Attack)
        {
            result = ResolvePlayerAttack(player, monster);
        }
        else if (playerAction == CombatActionType.Charge)
        {
            _charge.DoCharge(player, _config);
            _log.Add($"玩家蓄力：当前充能 {player.Charge}，溢出层 {player.OverflowCharge}");
        }

        ResolveMonsterAction(player, monster, monsterSkill);

        return result;
    }

    private ActionResolutionResult ResolvePlayerAttack(PlayerCombatActor player, MonsterCombatActor monster)
    {
        var roulette = _roulette.Spin(_config, player.Charge);
        var triggeredItems = new List<ItemData>();
        foreach (var zone in roulette.TriggeredZones)
        {
            if (player.RouletteZones.TryGetValue(zone, out var items))
            {
                triggeredItems.AddRange(items);
                _log.Add($"轮盘区域 {zone} 触发，道具：{(items.Count == 0 ? "无" : string.Join("/", items.Select(i => i.Name)))}");
            }
        }

        var baseDamage = roulette.TotalBaseDamage;
        var add = 0;
        foreach (var item in triggeredItems.Where(i => i.Phase == ItemResolutionPhase.Additive && i.MeetsZoneCondition(roulette.TriggerCount)))
        {
            add += item.FlatDamage;
            _log.Add($"道具加算 {item.Name}：+{item.FlatDamage}");
        }

        var damage = baseDamage + add;
        var multiplier = 1f;
        foreach (var item in triggeredItems.Where(i => i.Phase == ItemResolutionPhase.Multiplier && i.MeetsZoneCondition(roulette.TriggerCount)))
        {
            multiplier *= item.DamageMultiplier;
            _log.Add($"道具倍率 {item.Name}：x{item.DamageMultiplier:0.##}");
        }

        if (player.OverflowCharge > 0)
        {
            var overflowMultiplier = 1f + player.OverflowCharge * _config.OverflowDamageBonusPerLayer;
            multiplier *= overflowMultiplier;
            _log.Add($"溢出蓄力倍率：x{overflowMultiplier:0.##}");
        }

        damage = (int)MathF.Round(damage * multiplier);

        var echo = triggeredItems.Count(i => i.Phase == ItemResolutionPhase.ExtraTrigger && i.EchoZoneDamage);
        if (echo > 0)
        {
            damage *= (1 + echo);
            _log.Add($"追加触发（回响）次数 {echo}，伤害变为 {damage}");
        }

        foreach (var tech in player.EquippedTechniques)
        {
            if (TechniqueMatched(player, tech))
            {
                damage += tech.BonusDamage;
                _log.Add($"格斗技触发 {tech.Name}：+{tech.BonusDamage}");
                break;
            }
        }

        damage = _status.ApplyOutgoingDamageModifiers(player, damage);
        damage = _status.ApplyIncomingDamageModifiers(monster, damage);
        if (monster.IsCharging)
        {
            damage = (int)MathF.Round(damage * _config.ChargeHitDamageMultiplier);
            _log.Add("命中蓄力目标：伤害 x1.5");
        }

        var defend = _defense.Resolve(damage, monster, _config);
        var hpDamage = monster.ApplyDamage(defend.DamageAfterDefense);
        monster.DefendSuccessThisAction = defend.DefendSuccess;
        _log.Add($"玩家攻击结算：基础{baseDamage} +加算{add}，最终伤害 {defend.DamageAfterDefense}，造成生命伤害 {hpDamage}");

        var interrupt = roulette.TriggerCount * _config.DefaultInterruptPerZone;
        foreach (var item in triggeredItems.Where(i => i.Phase == ItemResolutionPhase.OnHit && i.ExtraInterrupt > 0))
        {
            interrupt += item.ExtraInterrupt;
            _log.Add($"道具打断 {item.Name}：+{item.ExtraInterrupt}");
        }

        ApplyOnHitStatuses(triggeredItems, monster);
        TryInterrupt(monster, interrupt, "怪物");

        var shieldRate = (1f / 3f) * (1f + player.OverflowCharge * _config.OverflowShieldBonusPerLayer);
        var gainedShield = (int)MathF.Round(defend.DamageAfterDefense * shieldRate);
        player.GainShield(gainedShield);
        _log.Add($"攻击转盾：+{gainedShield}（当前护盾 {player.Shield}）");

        foreach (var item in triggeredItems.Where(i => i.Phase == ItemResolutionPhase.Finisher && i.ExtraShieldAfterAttack > 0))
        {
            player.GainShield(item.ExtraShieldAfterAttack);
            _log.Add($"收尾护盾 {item.Name}：+{item.ExtraShieldAfterAttack}");
        }

        _charge.ClearAfterAttack(player);

        var extraSources = new List<string>();
        foreach (var item in triggeredItems.Where(i => i.GrantsExtraTurnOnThreeZones && roulette.TriggerCount == 3))
        {
            extraSources.Add(item.Id);
        }

        return new ActionResolutionResult
        {
            ExtraTurnSources = extraSources
        };
    }

    private void ResolveMonsterAction(PlayerCombatActor player, MonsterCombatActor monster, MonsterSkillData skill)
    {
        if (skill.ActionType == CombatActionType.Charge)
        {
            monster.IsCharging = true;
            monster.ChargedSkillId = skill.ChargedSkillId;
            monster.PendingIntent = skill.IntentText ?? "即将释放技能";
            _log.Add($"怪物蓄力：{monster.PendingIntent}");
            return;
        }

        if (skill.ActionType == CombatActionType.Defend)
        {
            monster.IsDefending = true;
            monster.GainShield(skill.ShieldGain);
            _log.Add($"怪物防御：护盾 +{skill.ShieldGain}");
            return;
        }

        if (skill.ActionType != CombatActionType.Attack) return;

        monster.IsCharging = false;
        monster.ChargedSkillId = null;
        monster.PendingIntent = "";

        for (var i = 0; i < skill.HitCount; i++)
        {
            var raw = _status.ApplyOutgoingDamageModifiers(monster, skill.DamagePerHit);
            raw = _status.ApplyIncomingDamageModifiers(player, raw);

            if (player.IsCharging)
            {
                raw = (int)MathF.Round(raw * _config.ChargeHitDamageMultiplier);
            }

            var defend = _defense.Resolve(raw, player, _config);
            var hp = player.ApplyDamage(defend.DamageAfterDefense);
            player.DefendSuccessThisAction = defend.DefendSuccess;
            _log.Add($"怪物技能 {skill.Name} 第{i + 1}段：原始{raw}，结算{defend.DamageAfterDefense}，玩家掉血 {hp}");

            if (skill.PoisonOnHit > 0) player.AddStatus(StatusEffectType.Poison, skill.PoisonOnHit);
            if (skill.BleedOnHit > 0) player.AddStatus(StatusEffectType.Bleed, skill.BleedOnHit);
            if (skill.WeakOnHit > 0) player.AddStatus(StatusEffectType.Weak, skill.WeakOnHit);
            if (skill.VulnerableOnHit > 0) player.AddStatus(StatusEffectType.Vulnerable, skill.VulnerableOnHit);

            var interrupt = skill.InterruptValue > 0 ? skill.InterruptValue : 8;
            TryInterrupt(player, interrupt, "玩家");
        }
    }

    private static bool TechniqueMatched(PlayerCombatActor player, TechniqueData technique)
    {
        if (player.ActionHistory.Count < technique.Sequence.Length) return false;
        for (var i = 0; i < technique.Sequence.Length; i++)
        {
            var c = player.ActionHistory[player.ActionHistory.Count - technique.Sequence.Length + i];
            if (c != technique.Sequence[i]) return false;
        }
        return true;
    }

    private void ApplyOnHitStatuses(IEnumerable<ItemData> items, MonsterCombatActor target)
    {
        foreach (var item in items.Where(i => i.Phase == ItemResolutionPhase.OnHit))
        {
            if (item.BleedOnHit > 0)
            {
                target.AddStatus(StatusEffectType.Bleed, item.BleedOnHit);
                _log.Add($"命中附加：{item.Name} 施加流血 {item.BleedOnHit}");
            }
            if (item.PoisonOnHit > 0)
            {
                target.AddStatus(StatusEffectType.Poison, item.PoisonOnHit);
                _log.Add($"命中附加：{item.Name} 施加中毒 {item.PoisonOnHit}");
            }
            if (item.VulnerableOnHit > 0)
            {
                target.AddStatus(StatusEffectType.Vulnerable, item.VulnerableOnHit);
                _log.Add($"命中附加：{item.Name} 施加易伤 {item.VulnerableOnHit}");
            }
        }
    }

    private void TryInterrupt(CombatActor actor, int value, string actorSide)
    {
        if (!actor.IsCharging) return;

        actor.AddInterrupt(value);
        _log.Add($"{actorSide} 韧性条 +{value} => {actor.InterruptMeter}/{actor.Tenacity}");
        if (actor.InterruptMeter < actor.Tenacity) return;

        actor.IsCharging = false;
        actor.ResetInterrupt();
        actor.AddStatus(StatusEffectType.Stun, 1);
        if (actor is PlayerCombatActor p)
        {
            _charge.ClearAfterInterrupt(p);
        }

        _log.Add($"{actorSide} 蓄力被打断，眩晕 1 回合，蓄力清空");
    }
}

public sealed class ActionResolutionResult
{
    public List<string> ExtraTurnSources { get; init; } = new();
}
