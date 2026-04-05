using System;

namespace DemoDunjia.Data;

public sealed class ItemData
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public ItemResolutionPhase Phase { get; init; }

    public int FlatDamage { get; init; }
    public float DamageMultiplier { get; init; } = 1f;
    public int ExtraInterrupt { get; init; }
    public bool EchoZoneDamage { get; init; }
    public int ExtraShieldAfterAttack { get; init; }

    public int BleedOnHit { get; init; }
    public int PoisonOnHit { get; init; }
    public int VulnerableOnHit { get; init; }

    public int MinTriggeredZones { get; init; }
    public int ExactTriggeredZones { get; init; }

    public bool GrantsExtraTurnOnThreeZones { get; init; }

    public bool MeetsZoneCondition(int triggeredZones)
    {
        if (ExactTriggeredZones > 0 && ExactTriggeredZones != triggeredZones)
        {
            return false;
        }

        if (MinTriggeredZones > 0 && triggeredZones < MinTriggeredZones)
        {
            return false;
        }

        return true;
    }

    public static ItemData Additive(string id, string name, int flatDamage, int minZones = 0, int exactZones = 0)
    {
        return new ItemData
        {
            Id = id,
            Name = name,
            Phase = ItemResolutionPhase.Additive,
            FlatDamage = flatDamage,
            MinTriggeredZones = minZones,
            ExactTriggeredZones = exactZones
        };
    }
}
