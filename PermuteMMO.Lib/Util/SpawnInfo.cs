using System.Diagnostics.CodeAnalysis;
using PKHeX.Core;

namespace PermuteMMO.Lib;

/// <summary>
/// Top-level spawner details to feed into the permutation logic.
/// </summary>
public sealed record SpawnInfo(SpawnCount Count, SpawnSet Set, SpawnType Type, SpawnInfo? Next = null)
{
    private static readonly SpawnCount MMO = new(4, 4);
    private static readonly SpawnCount Outbreak = new(4, 4);

    private SpawnInfo? Next { get; set; } = Next;
    public bool NoMultiAlpha => Type is SpawnType.Regular;
    public bool AllowGhosts => Type is not SpawnType.Regular;
    public bool RetainExisting => Type is SpawnType.Regular;

    public string GetSummary(string prefix)
    {
        var summary = $"{prefix}{this}";
        if (Next is not { } x)
            return summary;
        if (ReferenceEquals(this, x))
            return summary + " REPEATING.";
        return summary + Environment.NewLine + x.GetSummary(prefix);
    }

    public bool GetNextWave([NotNullWhen(true)] out SpawnInfo? next) => (next = Next) != null;

    public SpawnInfo(MassiveOutbreakSpawner8a spawner) : this(MMO, new SpawnSet(spawner.BaseTable, spawner.BaseCount), SpawnType.MMO, GetBonusChain(spawner)) { }
    public SpawnInfo(MassOutbreakSpawner8a spawner) : this(Outbreak, new SpawnSet(spawner.DisplaySpecies, spawner.BaseCount), SpawnType.Outbreak) { }

    public static SpawnInfo GetMMO(ulong baseTable, in int baseCount, ulong bonusTable, in int bonusCount)
    {
        var child = new SpawnInfo(MMO, new SpawnSet(bonusTable, bonusCount), SpawnType.MMO);
        return new SpawnInfo(MMO, new SpawnSet(baseTable, baseCount), SpawnType.MMO, child);
    }

    public SpawnState GetStartingState()
    {
        if (Type is SpawnType.Regular)
            return SpawnState.Get(Count.GetNextCount());
        return SpawnState.Get(Set.Count, Count.MaxAlive);
    }

    private static SpawnInfo? GetBonusChain(MassiveOutbreakSpawner8a spawner)
    {
        if (!spawner.HasBonus)
            return null;
        return new SpawnInfo(MMO, new SpawnSet(spawner.BonusTable, spawner.BonusCount), SpawnType.MMO);
    }

    public static SpawnInfo GetMO(ulong table, int count) => new(Outbreak, new SpawnSet(table, count), SpawnType.Outbreak);

    public static SpawnInfo GetLoop(SpawnCount count, SpawnSet set, SpawnType type)
    {
        var result = new SpawnInfo(count, set, type);
        result.Next = result;
        return result;
    }
}

public readonly record struct SpawnSet(ulong Table, int Count);

public record SpawnCount(int MaxAlive, int MinAlive = 0, ulong CountSeed = 0)
{
    public ulong CountSeed { get; set; } = CountSeed;

    public bool IsFixedCount => MinAlive is 0 || MinAlive == MaxAlive;

    public int GetNextCount()
    {
        if (IsFixedCount)
            return MaxAlive;
        var rand = new Xoroshiro128Plus(CountSeed);
        var delta = MaxAlive - MinAlive;
        var result = MinAlive + (int)rand.NextInt((uint)delta + 1);
        CountSeed = rand.Next();
        return result;
    }

    private int PeekNextCount()
    {
        var rand = new Xoroshiro128Plus(CountSeed);
        var delta = MaxAlive - MinAlive;
        return MinAlive + (int)rand.NextInt((uint)delta + 1);
    }

    public bool CanSpawnMore(int currentMaxAlive)
    {
        if (IsFixedCount)
            return false;

        var nextMaxAlive = PeekNextCount();
        if (nextMaxAlive > currentMaxAlive)
            return true;
        return nextMaxAlive == currentMaxAlive && nextMaxAlive != MaxAlive;
    }
}

public static class HashUtil
{
    public static bool IsNonZeroHash(in ulong hash) => hash is not (0 or 0xCBF29CE484222645);
}
