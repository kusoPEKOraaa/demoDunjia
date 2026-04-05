using System;
using System.Collections.Generic;
using DemoDunjia.Data;

namespace DemoDunjia.Systems;

public sealed class RouletteResult
{
    public required List<int> TriggeredZones { get; init; }
    public int TotalBaseDamage { get; init; }
    public int TriggerCount => TriggeredZones.Count;
}

public sealed class RouletteSystem
{
    private readonly Random _random = new();

    public RouletteResult Spin(BattleConfig config, int charge)
    {
        var triggerCount = charge switch
        {
            <= 0 => 1,
            1 => 2,
            _ => 3
        };

        var pool = new List<int> { 1, 2, 3 };
        var selected = new List<int>();
        for (var i = 0; i < triggerCount && pool.Count > 0; i++)
        {
            var idx = _random.Next(pool.Count);
            selected.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        selected.Sort(); // 1->2->3
        return new RouletteResult
        {
            TriggeredZones = selected,
            TotalBaseDamage = selected.Count * config.RouletteBaseValuePerZone
        };
    }
}
