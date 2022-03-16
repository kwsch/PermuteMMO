namespace PermuteMMO.Lib;

/// <summary>
/// Top-level spawner details to feed into the permutation logic.
/// </summary>
public sealed record SpawnInfo : ISpawnInfo
{
    public int BaseCount { get; init; }
    public int BonusCount { get; init; }
    public ulong BaseTable { get; init; }
    public ulong BonusTable { get; init; }
}
