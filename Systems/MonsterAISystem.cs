using DemoDunjia.Data;
using DemoDunjia.Entities;

namespace DemoDunjia.Systems;

public sealed class MonsterAISystem
{
    public MonsterSkillData ChooseSkill(MonsterCombatActor monster, PlayerCombatActor player)
    {
        if (monster.ChargedSkillId is { } charged)
        {
            return monster.Data.Skills[charged];
        }

        return monster.Data.Id switch
        {
            "fang_beast" => ChooseFangBeast(monster, player),
            "burst_bug" => ChooseBurstBug(monster, player),
            "poison_spore" => ChoosePoisonSpore(monster, player),
            "spine_hunter" => ChooseSpineHunter(monster, player),
            "tri_gate" => ChooseTriGate(monster, player),
            _ => monster.Data.Skills["attack"]
        };
    }

    private static MonsterSkillData ChooseFangBeast(MonsterCombatActor monster, PlayerCombatActor player)
    {
        if (player.Shield <= 2)
        {
            return monster.Data.Skills["pounce"];
        }
        return monster.Data.Skills["bite"];
    }

    private static MonsterSkillData ChooseBurstBug(MonsterCombatActor monster, PlayerCombatActor player)
    {
        if (player.GetStatus(StatusEffectType.Stun) > 0)
        {
            return monster.Data.Skills["probe"];
        }

        return monster.Data.Skills["charge"];
    }

    private static MonsterSkillData ChoosePoisonSpore(MonsterCombatActor monster, PlayerCombatActor player)
    {
        if (player.GetStatus(StatusEffectType.Poison) < 2)
        {
            return monster.Data.Skills["spray"];
        }

        return monster.Data.Skills["spike"];
    }

    private static MonsterSkillData ChooseSpineHunter(MonsterCombatActor monster, PlayerCombatActor player)
    {
        if (player.GetStatus(StatusEffectType.Vulnerable) == 0)
        {
            return monster.Data.Skills["mark"];
        }

        if (monster.Hp < monster.MaxHp / 2)
        {
            return monster.Data.Skills["charge_hunt"];
        }

        return monster.Data.Skills["flurry"];
    }

    private static MonsterSkillData ChooseTriGate(MonsterCombatActor monster, PlayerCombatActor player)
    {
        var ratio = (float)monster.Hp / monster.MaxHp;
        if (ratio > 0.67f)
        {
            monster.CurrentPhase = "攻势相";
            return monster.Data.Skills["dual"];
        }

        if (ratio > 0.33f)
        {
            monster.CurrentPhase = "蓄势相";
            if (monster.ChargedSkillId is null)
            {
                return monster.Data.Skills["core_charge"];
            }
            return monster.Data.Skills[monster.ChargedSkillId];
        }

        monster.CurrentPhase = "守势相";
        if (player.ActionHistory.Count > 0 && player.ActionHistory[^1] == 'A')
        {
            return monster.Data.Skills["fortify"];
        }

        return monster.Data.Skills["heavy"];
    }
}
