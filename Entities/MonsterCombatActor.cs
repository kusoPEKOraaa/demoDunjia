using DemoDunjia.Data;

namespace DemoDunjia.Entities;

public sealed class MonsterCombatActor : CombatActor
{
    public MonsterData Data { get; private set; } = null!;
    public string? ChargedSkillId { get; set; }
    public string CurrentPhase { get; set; } = "";

    public void Setup(MonsterData data)
    {
        Data = data;
        CurrentPhase = data.Phases.Length > 0 ? data.Phases[0] : "Normal";
        Initialize(data.Name, data.MaxHp, 999, 0, data.Tenacity);
        ChargedSkillId = null;
    }
}
