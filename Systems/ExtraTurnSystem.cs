using System.Collections.Generic;

namespace DemoDunjia.Systems;

public sealed class ExtraTurnSystem
{
    private readonly HashSet<string> _usedSources = new();

    public bool TryConsumeSource(string source)
    {
        if (_usedSources.Contains(source)) return false;
        _usedSources.Add(source);
        return true;
    }

    public void ResetBattle()
    {
        _usedSources.Clear();
    }
}
