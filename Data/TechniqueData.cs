namespace DemoDunjia.Data;

public sealed class TechniqueData
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Sequence { get; init; } // A,D,N
    public int BonusDamage { get; init; }
}
