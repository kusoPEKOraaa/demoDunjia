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

    public BattleConfig Config { get; private set; } = null!;
    public CombatLog Log { get; private set; } = null!;
    public PlayerCombatActor Player { get; private set; } = null!;
    public MonsterCombatActor Monster { get; private set; } = null!;
    public int Turn { get; private set; } = 1;

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

        Player.Setup(Config, carryOverShield: 0);
        var items = DemoContentFactory.CreateStartingItems();
        Player.RouletteZones[1].AddRange(items.GetRange(0, 3));
        Player.RouletteZones[2].AddRange(items.GetRange(3, 3));
        Player.RouletteZones[3].AddRange(items.GetRange(6, 2));
        Player.EquippedTechniques.AddRange(DemoContentFactory.CreateDefaultTechniques());

        LoadMonster(_monsters[_monsterIndex]);
        Turn = 1;
        Log.Clear();
        Log.Add("战斗开始（垂直切片）");
        EmitSignal(SignalName.StateChanged);
    }

    public bool CanDefend() => Player.ConsecutiveDefendCount < Config.MaxConsecutiveDefend;

    public void PlayerSelectAction(CombatActionType action)
    {
        if (Player.IsDead || Monster.IsDead) return;
        if (action == CombatActionType.Defend && !CanDefend())
        {
            Log.Add("连续防御达到上限，本回合不能防御。");
            EmitSignal(SignalName.StateChanged);
            return;
        }

        _turnResolver.ResolveTurn(Turn, Player, Monster, action);
        if (Monster.IsDead)
        {
            Log.Add($"怪物 {Monster.Name} 被击败。");
            TryLoadNextMonster();
        }

        if (Player.IsDead)
        {
            Log.Add("玩家倒下，战斗失败。");
        }

        Turn += 1;
        EmitSignal(SignalName.StateChanged);
    }

    private void TryLoadNextMonster()
    {
        _monsterIndex += 1;
        if (_monsterIndex >= _monsters.Count)
        {
            Log.Add("已完成 5 个首批怪物战斗切片。Demo 胜利。");
            return;
        }

        var carryShield = Math.Min(Player.Shield, Config.CarryOverShieldCap);
        Player.Setup(Config, carryShield);
        var items = DemoContentFactory.CreateStartingItems();
        Player.RouletteZones[1].AddRange(items.GetRange(0, 3));
        Player.RouletteZones[2].AddRange(items.GetRange(3, 3));
        Player.RouletteZones[3].AddRange(items.GetRange(6, 2));
        Player.EquippedTechniques.AddRange(DemoContentFactory.CreateDefaultTechniques());
        LoadMonster(_monsters[_monsterIndex]);
        Log.Add($"进入下一战：{Monster.Name}");
    }

    private void LoadMonster(MonsterData data)
    {
        Monster.Setup(data);
    }
}
