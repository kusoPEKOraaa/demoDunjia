using System.Collections.Generic;
using DemoDunjia.Data;
using DemoDunjia.Entities;
using DemoDunjia.Systems;

namespace DemoDunjia.Combat;

public sealed class TurnResolver
{
    private readonly CombatLog _log;
    private readonly StatusEffectSystem _status;
    private readonly ExtraTurnSystem _extraTurns;
    private readonly MonsterAISystem _ai;
    private readonly ActionResolver _actions;

    public TurnResolver(CombatLog log, StatusEffectSystem status, ExtraTurnSystem extraTurns, MonsterAISystem ai, ActionResolver actions)
    {
        _log = log;
        _status = status;
        _extraTurns = extraTurns;
        _ai = ai;
        _actions = actions;
    }

    public void ResolveTurn(int turn, PlayerCombatActor player, MonsterCombatActor monster, CombatActionType playerAction)
    {
        _log.Add($"==== 回合 {turn} 开始 ====");

        var playerStunned = _status.ConsumeStun(player);
        var monsterStunned = _status.ConsumeStun(monster);

        if (playerStunned)
        {
            playerAction = CombatActionType.Stunned;
            _log.Add("玩家眩晕，本回合空过");
        }

        var monsterSkill = monsterStunned
            ? new MonsterSkillData { Id = "stunned", Name = "眩晕空过", ActionType = CombatActionType.Stunned }
            : _ai.ChooseSkill(monster, player);

        _log.Add($"双方行动：玩家={playerAction}，怪物={monsterSkill.Name}");

        var normalAction = playerAction == CombatActionType.Stunned ? CombatActionType.None : playerAction;
        var actionResult = _actions.ResolvePlayerAction(player, monster, normalAction, monsterSkill);
        RecordPlayerAction(player, playerAction);

        ResolveDots(player, monster);

        // 追加行动：插入玩家额外行动，怪物行动不刷新
        foreach (var source in actionResult.ExtraTurnSources)
        {
            if (!_extraTurns.TryConsumeSource(source)) continue;
            _log.Add($"触发追加行动，来源：{source}");
            var followUp = CombatActionType.Attack;
            _actions.ResolvePlayerAction(player, monster, followUp, new MonsterSkillData { Id = "extra_none", Name = "追加行动期间怪物不动作", ActionType = CombatActionType.None });
            RecordPlayerAction(player, followUp);
            ResolveDots(player, monster);
        }

        _log.Add($"回合结束：玩家 HP {player.Hp}/{player.MaxHp} 护盾 {player.Shield}；怪物 HP {monster.Hp}/{monster.MaxHp}");
    }

    private void ResolveDots(PlayerCombatActor player, MonsterCombatActor monster)
    {
        _status.ApplyEndTurnDot(player, _log, "玩家");
        _status.ApplyEndTurnDot(monster, _log, "怪物");
    }

    private static void RecordPlayerAction(PlayerCombatActor player, CombatActionType action)
    {
        var symbol = action switch
        {
            CombatActionType.Attack => 'A',
            CombatActionType.Defend => 'D',
            _ => 'N'
        };

        player.ActionHistory.Add(symbol);
        if (player.ActionHistory.Count > 8)
        {
            player.ActionHistory.RemoveAt(0);
        }

        if (action == CombatActionType.Defend)
        {
            player.ConsecutiveDefendCount += 1;
        }
        else
        {
            player.ConsecutiveDefendCount = 0;
        }
    }
}
