namespace DemoDunjia.Data;

public sealed class BattleConfig
{
    public int PlayerMaxHp { get; init; } = 100;
    public int PlayerShieldCap { get; init; } = 40;
    public int CarryOverShieldCap { get; init; } = 10;
    public int BaseDefenseShieldGain { get; init; } = 8;
    public int ChargeCap { get; init; } = 2;
    public float OverflowDamageBonusPerLayer { get; init; } = 0.2f;
    public float OverflowShieldBonusPerLayer { get; init; } = 0.2f;
    public int RouletteBaseValuePerZone { get; init; } = 6;
    public int RouletteZoneCount { get; init; } = 3;
    public int DefaultInterruptPerZone { get; init; } = 8;
    public int PlayerTenacity { get; init; } = 20;
    public float DefendInitialReduction { get; init; } = 0.8f;
    public float DefendSuccessReduction { get; init; } = 0.64f;
    public float ChargeHitDamageMultiplier { get; init; } = 1.5f;
    public int MaxConsecutiveDefend { get; init; } = 2;
}
