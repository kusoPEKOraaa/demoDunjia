using System;
using System.Collections.Generic;
using DemoDunjia.Data;
using DemoDunjia.Entities;
using DemoDunjia.Systems;
using Godot;

namespace DemoDunjia.Combat;

public partial class BattleController : Node
{
    [Signal] public delegate void StateChangedEventHandler();
    [Signal] public delegate void PhaseChangedEventHandler(string phaseText);
    [Signal] public delegate void RouletteResolvedEventHandler(int[] zones);
    [Signal] public delegate void RouletteItemsResolvedEventHandler(Godot.Collections.Dictionary itemsByZone);
    [Signal] public delegate void BattleEndedEventHandler(bool victory, string message, int damageDealt, int damageTaken);

    public BattleConfig Config { get; private set; } = null!;
    public CombatLog Log { get; private set; } = null!;
    public PlayerCombatActor Player { get; private set; } = null!;
    public MonsterCombatActor Monster { get; private set; } = null!;
    public int Turn { get; private set; } = 1;
    public string PhaseText { get; private set; } = "等待玩家行动";
    public int TotalDamageDealt { get; private set; }
    public int TotalDamageTaken { get; private set; }
    public bool IsResolving { get; private set; }

    private TurnResolver _turnResolver = null!;
    private List<MonsterData> _monsters = null!;
    private int _monsterIndex;

    public override void _Ready()
    {
        Config = DemoContentFactory.CreateConfig();
        Log = new CombatLog();
        Player = new PlayerCombatActor();
        Monster = new MonsterCombatActor();

        var roulette = new RouletteSystem();
        var defense = new DefenseSystem();
        var status = new StatusEffectSystem();
        var charge = new ChargeSystem();
        var extraTurns = new ExtraTurnSystem();
        var ai = new MonsterAISystem();
        var actions = new ActionResolver(Config, Log, roulette, defense, status, charge);
        _turnResolver = new TurnResolver(Log, status, extraTurns, ai, actions);

        InitializeBattle();
    }

    public void InitializeBattle()
    {
        _monsters = DemoContentFactory.CreateMonsters();
        _monsterIndex = 0;

        SetupPlayerForNewBattle(0);
        LoadMonster(_monsters[_monsterIndex]);
        Turn = 1;
        TotalDamageDealt = 0;
        TotalDamageTaken = 0;
        IsResolving = false;
        PhaseText = "等待玩家行动";
        Log.Clear();
        Log.Add("战斗开始（垂直切片）", CombatLogCategory.Result);
        EmitSignal(SignalName.StateChanged);
        EmitSignal(SignalName.PhaseChanged, PhaseText);
    }

    public bool CanDefend() => Player.ConsecutiveDefendCount < Config.MaxConsecutiveDefend;

    public void PlayerSelectAction(CombatActionType action)
    {
        if (IsResolving || Player.IsDead || Monster.IsDead) return;
        if (action == CombatActionType.Defend && !CanDefend())
        {
            Log.Add("连续防御达到上限，本回合不能防御。", CombatLogCategory.Action);
            EmitSignal(SignalName.StateChanged);
            return;
        }

        IsResolving = true;
        SetPhase("结算中");
        var oldMonsterHp = Monster.Hp;
        var oldPlayerHp = Player.Hp;
        var summary = _turnResolver.ResolveTurn(Turn, Player, Monster, action);
        TotalDamageDealt += Math.Max(0, oldMonsterHp - Monster.Hp);
        TotalDamageTaken += Math.Max(0, oldPlayerHp - Player.Hp);

        if (summary.PrimaryAction.TriggeredZones.Count > 0)
        {
            EmitSignal(SignalName.RouletteResolved, summary.PrimaryAction.TriggeredZones.ToArray());
            EmitSignal(SignalName.RouletteItemsResolved, ToGodotItemMap(summary.PrimaryAction.TriggeredItemsByZone));
        }

        foreach (var extra in summary.ExtraActions)
        {
            SetPhase("追加行动中");
            EmitSignal(SignalName.RouletteResolved, extra.TriggeredZones.ToArray());
            EmitSignal(SignalName.RouletteItemsResolved, ToGodotItemMap(extra.TriggeredItemsByZone));
        }

        if (Monster.IsDead)
        {
            Log.Add($"怪物 {Monster.Name} 被击败。", CombatLogCategory.Result);
            if (!TryLoadNextMonster())
            {
                SetPhase("战斗结束");
                EmitSignal(SignalName.BattleEnded, true, "已完成 5 个首批怪物战斗切片。Demo 胜利。", TotalDamageDealt, TotalDamageTaken);
            }
        }

        if (Player.IsDead)
        {
            Log.Add("玩家倒下，战斗失败。", CombatLogCategory.Result);
            SetPhase("战斗结束");
            EmitSignal(SignalName.BattleEnded, false, "战斗失败", TotalDamageDealt, TotalDamageTaken);
        }

        Turn += 1;
        IsResolving = false;
        if (!Player.IsDead)
        {
            SetPhase("等待玩家行动");
        }
        EmitSignal(SignalName.StateChanged);

        if (!Player.IsDead && !Monster.IsDead && Player.HasStatus(StatusEffectType.Stun))
        {
            CallDeferred(MethodName.ResolveStunnedTurn);
        }
    }

    public string GetWeaponName() => "训练短刃（占位）";

    public string GetShieldName() => "基础护盾（占位）";

    private void ResolveStunnedTurn()
    {
        if (IsResolving || Player.IsDead || Monster.IsDead || !Player.HasStatus(StatusEffectType.Stun)) return;
        Log.Add("检测到玩家眩晕：自动结算空过回合。", CombatLogCategory.Status);
        PlayerSelectAction(CombatActionType.None);
    }


    private static Godot.Collections.Dictionary ToGodotItemMap(Dictionary<int, List<string>> source)
    {
        var dict = new Godot.Collections.Dictionary();
        foreach (var kv in source)
        {
            var arr = new Godot.Collections.Array<string>();
            foreach (var item in kv.Value) arr.Add(item);
            dict[kv.Key] = arr;
        }

        return dict;
    }

    private bool TryLoadNextMonster()
    {
        _monsterIndex += 1;
        if (_monsterIndex >= _monsters.Count)
        {
            Log.Add("已完成 5 个首批怪物战斗切片。Demo 胜利。", CombatLogCategory.Result);
            return false;
        }

        var carryShield = Math.Min(Player.Shield, Config.CarryOverShieldCap);
        SetupPlayerForNewBattle(carryShield);
        LoadMonster(_monsters[_monsterIndex]);
        Log.Add($"进入下一战：{Monster.Name}", CombatLogCategory.Result);
        return true;
    }

    private void SetupPlayerForNewBattle(int carryShield)
    {
        Player.Setup(Config, carryShield);
        var items = DemoContentFactory.CreateStartingItems();
        Player.RouletteZones[1].AddRange(items.GetRange(0, 3));
        Player.RouletteZones[2].AddRange(items.GetRange(3, 3));
        Player.RouletteZones[3].AddRange(items.GetRange(6, 2));
        Player.EquippedTechniques.AddRange(DemoContentFactory.CreateDefaultTechniques());
    }

    private void LoadMonster(MonsterData data)
    {
        Monster.Setup(data);
    }

    private void SetPhase(string text)
    {
        PhaseText = text;
        EmitSignal(SignalName.PhaseChanged, text);
    }
}
