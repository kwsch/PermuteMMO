using PKHeX.Core;

namespace PermuteMMO.Lib;

/// <summary>
/// Runs simple forwards calculations for seeds.
/// </summary>
public static class Calculations
{
    /// <summary>
    /// Gets the group seed value after the provided advance steps.
    /// </summary>
    public static ulong GetGroupSeed(in ulong groupSeed, IEnumerable<Advance> advances)
    {
        ulong seed = GetGroupSeed(groupSeed, 4);
        foreach (var advance in advances)
        {
            var count = advance.AdvanceCount();
            var rng = new Xoroshiro128Plus(seed);
            for (int i = 0; i < count; i++)
            {
                _ = rng.Next(); // generate/slot seed
                _ = rng.Next(); // alpha move
            }
            seed = rng.Next(); // Reset the seed for future spawns.
        }

        return seed;
    }

    /// <summary>
    /// Gets the group seed value after spawning the specified count of entities.
    /// </summary>
    public static ulong GetGroupSeed(ulong seed, int count)
    {
        var rng = new Xoroshiro128Plus(seed);
        for (int i = 0; i < count; i++)
        {
            _ = rng.Next(); // generate/slot seed
            _ = rng.Next(); // alpha move
        }

        return rng.Next(); // Reset the seed for future spawns.
    }

    /// <summary>
    /// Gets the slot generation seed (slot, entity seed) that re-spawns at a 1-based index.
    /// </summary>
    /// <param name="groupSeed">Group seed to spawn things for this regeneration round.</param>
    /// <param name="spawnIndex">1-indexed (not 0) spawn index.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static (ulong Generate, ulong Alpha) GetGenerateSeed(in ulong groupSeed, in int spawnIndex)
    {
        var rng = new Xoroshiro128Plus(groupSeed);
        for (int i = 1; i <= spawnIndex; i++)
        {
            var subSeed = rng.Next(); // generate/slot seed
            var alpha = rng.Next(); // alpha move, don't care

            if (i == spawnIndex)
                return (subSeed, alpha);
        }

        throw new ArgumentOutOfRangeException(nameof(spawnIndex));
    }

    /// <summary>
    /// Gets the entity template seed (slot, entity seed) that re-spawns at a 1-based index.
    /// </summary>
    /// <param name="groupSeed">Group seed to spawn things for this regeneration round.</param>
    /// <param name="spawnIndex">1-indexed (not 0) spawn index.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static ulong GetEntitySeed(in ulong groupSeed, in int spawnIndex)
    {
        var rng = new Xoroshiro128Plus(groupSeed);
        for (int i = 1; i <= spawnIndex; i++)
        {
            var subSeed = rng.Next(); // generate/slot seed
            _ = rng.Next(); // alpha move, don't care

            if (i != spawnIndex)
                continue;

            var poke = new Xoroshiro128Plus(subSeed);
            _ = poke.Next(); // slot
            return poke.Next();
        }

        throw new ArgumentOutOfRangeException(nameof(spawnIndex));
    }
}
