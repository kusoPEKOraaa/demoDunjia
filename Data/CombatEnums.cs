namespace DemoDunjia.Data;

public enum CombatActionType
{
    None,
    Attack,
    Charge,
    Defend,
    Stunned
}

public enum StatusEffectType
{
    Bleed,
    Poison,
    Weak,
    Vulnerable,
    Stun
}

public enum ItemResolutionPhase
{
    Additive,
    Multiplier,
    ExtraTrigger,
    OnHit,
    Finisher
}
