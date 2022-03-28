namespace PermuteMMO.Lib;

/// <summary>
/// Top-level spawner details to feed into the permutation logic.
/// </summary>
public sealed record SpawnInfo
{
    public int BaseCount { get; init; }
    public int BonusCount { get; init; }
    public ulong BaseTable { get; init; }
    public ulong BonusTable { get; init; }
    public SpawnType Type { get; init; } = SpawnType.MMO;

    public bool HasBase => BaseTable is not (0 or 0xCBF29CE484222645);
    public bool HasBonus => BonusTable is not (0 or 0xCBF29CE484222645);
}

public sealed record UserEnteredSpawnInfo
{
    public ulong Seed { get; set; }
    public ushort Species { get; set; }

    public int BaseCount { get; set; }
    public int BonusCount { get; set; }
    public ulong BaseTable { get; set; }
    public ulong BonusTable { get; set; }

    public SpawnInfo GetSpawn() => new()
    {
        BaseCount = BaseCount,
        BonusCount = BonusCount,
        BaseTable = BaseTable,
        BonusTable = BonusTable,
        Type = BonusTable is not (0 or 0xCBF29CE484222645) ? SpawnType.MMO : SpawnType.Outbreak,
    };
}
