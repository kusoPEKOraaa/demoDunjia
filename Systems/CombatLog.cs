using System;
using System.Collections.Generic;

namespace DemoDunjia.Systems;

public sealed class CombatLog
{
    private readonly List<string> _entries = new();

    public event Action<string>? EntryAdded;

    public IReadOnlyList<string> Entries => _entries;

    public void Add(string message)
    {
        _entries.Add(message);
        EntryAdded?.Invoke(message);
    }

    public void Clear()
    {
        _entries.Clear();
        EntryAdded?.Invoke("-- 日志已清空 --");
    }
}
