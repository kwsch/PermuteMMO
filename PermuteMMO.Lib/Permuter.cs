using System.Diagnostics;
using PKHeX.Core;

namespace PermuteMMO.Lib;

/// <summary>
/// Master iterator
/// </summary>
public static class Permuter
{
    private const int MaxAlive = 4;
    private const int MaxKill = 4;
    private const int MaxGhosts = 3;

    // State tracking
    private readonly record struct SpawnState(in int Count, in int Alive = 0, in int Dead = 0, in int Ghost = 0);

    /// <summary>
    /// Iterates through all possible player actions with the starting <see cref="seed"/> and <see cref="spawner"/> details.
    /// </summary>
    public static PermuteMeta Permute(SpawnInfo spawner, ulong seed)
    {
        var info = new PermuteMeta(spawner);
        var state = new SpawnState(spawner.BaseCount);

        // Generate the encounters!
        PermuteRecursion(info, spawner.BaseTable, false, seed, state);
        return info;
    }

    private static void PermuteRecursion(PermuteMeta spawn, in ulong table, in bool isBonus, in ulong seed, in SpawnState state)
    {
        // If the outbreak is not done, continue.
        if (state.Count != 0)
            PermuteOutbreak(spawn, table, isBonus, seed, state);
        else if (!isBonus && spawn.Spawner.HasBonus)
            PermuteFinish(spawn, table, seed, state);
        // Outbreak complete.
    }

    private static void PermuteOutbreak(PermuteMeta meta, in ulong table, in bool isBonus, in ulong seed, in SpawnState state)
    {
        // Re-spawn to capacity
        var respawn = Math.Min(state.Count, MaxAlive - state.Alive);
        Debug.Assert(respawn != 0);
        var reseed = GenerateSpawns(meta, table, isBonus, seed, respawn);

        // Update spawn state
        var newState = state with { Count = state.Count - respawn, Alive = MaxAlive };
        ContinuePermute(meta, table, isBonus, reseed, newState);
    }

    private static void ContinuePermute(PermuteMeta meta, in ulong table, in bool isBonus, in ulong seed, in SpawnState state)
    {
        // Check if we're now out of possible re-spawns
        if (state.Count == 0)
        {
            PermuteRecursion(meta, table, isBonus, seed, state);
            return;
        }

        // Permute our remaining options
        int canKO = state.Count >= MaxAlive ? MaxKill : state.Count;
        for (int i = 1; i <= canKO; i++)
        {
            var step = (int)Advance.A1 + (i - 1);
            meta.Start((Advance)step);
            var newState = state with { Alive = state.Alive - i, Dead = state.Dead + i };
            PermuteRecursion(meta, table, isBonus, seed, newState);
            meta.End();
        }
    }

    private static ulong GenerateSpawns(PermuteMeta spawn, in ulong table, in bool isBonus, in ulong seed, in int count)
    {
        var rng = new Xoroshiro128Plus(seed);
        for (int i = 1; i <= count; i++)
        {
            var subSeed = rng.Next();
            _ = rng.Next(); // Unknown

            var generate = SpawnGenerator.Generate(subSeed, table);
            if (spawn.IsResult(generate))
                spawn.AddResult(generate, i, isBonus);
        }
        return rng.Next(); // Reset the seed for future spawns.
    }

    private static void PermuteFinish(PermuteMeta meta, in ulong table, in ulong seed, in SpawnState state)
    {
        PermuteBonusTable(meta, seed);

        // Try adding ghost spawns if we haven't capped out yet.
        if (state.Ghost is not MaxGhosts)
            PermuteAddGhosts(meta, seed, table, state);
    }

    private static void PermuteBonusTable(PermuteMeta meta, in ulong seed)
    {
        meta.Start(Advance.SB);
        var state = new SpawnState(meta.Spawner.BonusCount);
        PermuteOutbreak(meta, meta.Spawner.BonusTable, true, seed, state);
        meta.End();
    }

    private static void PermuteAddGhosts(PermuteMeta meta, in ulong seed, in ulong table, in SpawnState state)
    {
        var remain = MaxAlive - state.Ghost;
        for (int i = 1; i < remain; i++)
        {
            // Get updated state with added ghosts
            var ghosts = state.Ghost + i;
            var newState = state with { Count = 0, Alive = state.Alive - i, Dead = state.Dead + i, Ghost = ghosts };
            var step = (int)Advance.G1 + (i - 1);

            // Simulate ghost advancements via camp reset
            var gSeed = GetGhostSeed(seed, ghosts);

            meta.Start((Advance)step);
            PermuteRecursion(meta, table, false, gSeed, newState);
            meta.End();
        }
    }

    private static ulong GetGhostSeed(in ulong seed, in int ghosts)
    {
        var rng = new Xoroshiro128Plus(seed);
        for (int g = 0; g < ghosts; g++)
        {
            _ = rng.Next();
            _ = rng.Next();
        }
        return rng.Next();
    }
}
