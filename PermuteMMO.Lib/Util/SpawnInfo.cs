using System.Diagnostics.CodeAnalysis;

namespace PermuteMMO.Lib;

/// <summary>
/// Top-level spawner details to feed into the permutation logic.
/// </summary>
public sealed record SpawnInfo(SpawnDetail Detail, SpawnSet Set, SpawnInfo? Next = null)
{
    private static readonly SpawnDetail MMO = new(SpawnType.MMO, 4);
    private static readonly SpawnDetail Outbreak = new(SpawnType.Outbreak, 4);

    private SpawnInfo? Next { get; set; } = Next;

    public bool GetNextWave([NotNullWhen(true)] out SpawnInfo? next) => (next = Next) != null;

    public SpawnInfo(MassiveOutbreakSpawner8a spawner) : this(MMO, new SpawnSet(spawner.BaseTable, spawner.BaseCount), GetBonusChain(spawner)) { }
    public SpawnInfo(MassOutbreakSpawner8a spawner) : this(Outbreak, new SpawnSet(spawner.DisplaySpecies, spawner.BaseCount)) { }

    public static SpawnInfo GetMMO(ulong baseTable, in int baseCount, ulong bonusTable, in int bonusCount)
    {
        var child = new SpawnInfo(MMO, new SpawnSet(bonusTable, bonusCount));
        return new SpawnInfo(MMO, new SpawnSet(baseTable, baseCount), child);
    }

    private static SpawnInfo? GetBonusChain(MassiveOutbreakSpawner8a spawner)
    {
        if (!spawner.HasBonus)
            return null;
        return new SpawnInfo(MMO, new SpawnSet(spawner.BonusTable, spawner.BonusCount));
    }

    public static SpawnInfo GetMO(ulong table, int count) => new(Outbreak, new SpawnSet(table, count));

    public static SpawnInfo GetLoop(SpawnDetail detail, SpawnSet set)
    {
        var result = new SpawnInfo(detail, set);
        result.Next = result;
        return result;
    }
}

public readonly record struct SpawnSet(ulong Table, int Count);

public readonly record struct SpawnDetail(SpawnType SpawnType, int MaxAlive);

public static class HashUtil
{
    public static bool IsNonZeroHash(in ulong hash) => hash is not (0 or 0xCBF29CE484222645);
}
