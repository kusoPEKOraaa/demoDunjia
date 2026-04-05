using System.Collections.Generic;

namespace DemoDunjia.Data;

public sealed class MonsterSkillData
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public CombatActionType ActionType { get; init; }
    public int DamagePerHit { get; init; }
    public int HitCount { get; init; } = 1;
    public int ShieldGain { get; init; }
    public int PoisonOnHit { get; init; }
    public int BleedOnHit { get; init; }
    public int VulnerableOnHit { get; init; }
    public int WeakOnHit { get; init; }
    public int InterruptValue { get; init; }
    public string? IntentText { get; init; }
    public string? ChargedSkillId { get; init; }
}

public sealed class MonsterData
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public int MaxHp { get; init; }
    public int Tenacity { get; init; } = 16;
    public bool IsBoss { get; init; }
    public string[] Phases { get; init; } = System.Array.Empty<string>();
    public required Dictionary<string, MonsterSkillData> Skills { get; init; }
}
