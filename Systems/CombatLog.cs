using System;
using System.Collections.Generic;

namespace DemoDunjia.Systems;

public enum CombatLogCategory
{
    General,
    Turn,
    Action,
    Damage,
    Shield,
    Status,
    Interrupt,
    ExtraTurn,
    Technique,
    Result
}

public readonly record struct CombatLogEntry(string Message, CombatLogCategory Category);

public sealed class CombatLog
{
    private readonly List<CombatLogEntry> _entries = new();

    public event Action<CombatLogEntry>? EntryAdded;

    public IReadOnlyList<CombatLogEntry> Entries => _entries;

    public void Add(string message, CombatLogCategory category = CombatLogCategory.General)
    {
        var entry = new CombatLogEntry(message, category);
        _entries.Add(entry);
        EntryAdded?.Invoke(entry);
    }

    public void Clear()
    {
        _entries.Clear();
        EntryAdded?.Invoke(new CombatLogEntry("-- 日志已清空 --", CombatLogCategory.General));
    }
}
