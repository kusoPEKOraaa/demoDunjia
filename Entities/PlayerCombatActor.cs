using System.Collections.Generic;
using DemoDunjia.Data;

namespace DemoDunjia.Entities;

public sealed class PlayerCombatActor : CombatActor
{
    public int Charge { get; set; }
    public int OverflowCharge { get; set; }
    public int ConsecutiveDefendCount { get; set; }

    public readonly Dictionary<int, List<ItemData>> RouletteZones = new();
    public readonly List<TechniqueData> EquippedTechniques = new();
    public readonly List<char> ActionHistory = new();

    public void Setup(BattleConfig config, int carryOverShield)
    {
        Initialize("玩家", config.PlayerMaxHp, config.PlayerShieldCap, carryOverShield, config.PlayerTenacity);
        RouletteZones.Clear();
        for (var i = 1; i <= config.RouletteZoneCount; i++)
        {
            RouletteZones[i] = new List<ItemData>();
        }
        Charge = 0;
        OverflowCharge = 0;
        ConsecutiveDefendCount = 0;
        ActionHistory.Clear();
        EquippedTechniques.Clear();
    }
}
