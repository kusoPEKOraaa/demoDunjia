using System;
using DemoDunjia.Data;
using DemoDunjia.Entities;

namespace DemoDunjia.Systems;

public sealed class DefenseResult
{
    public int RawDamage { get; init; }
    public int DamageAfterDefense { get; init; }
    public bool DefendSuccess { get; init; }
}

public sealed class DefenseSystem
{
    public DefenseResult Resolve(int rawDamage, CombatActor defender, BattleConfig config)
    {
        if (!defender.IsDefending)
        {
            return new DefenseResult { RawDamage = rawDamage, DamageAfterDefense = rawDamage, DefendSuccess = false };
        }

        var reduced80 = (int)MathF.Round(rawDamage * config.DefendInitialReduction);
        var reduced64 = (int)MathF.Round(rawDamage * config.DefendSuccessReduction);
        var success = defender.Shield >= reduced64;

        return new DefenseResult
        {
            RawDamage = rawDamage,
            DamageAfterDefense = success ? reduced64 : reduced80,
            DefendSuccess = success
        };
    }
}
