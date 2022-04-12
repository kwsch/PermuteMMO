using PKHeX.Core;

namespace PermuteMMO.Lib;

/// <summary>
/// Master iterator
/// </summary>
public static class Permuter
{
    /// <summary>
    /// Iterates through all possible player actions with the starting <see cref="seed"/> and <see cref="spawner"/> details.
    /// </summary>
    public static PermuteMeta Permute(SpawnInfo spawner, in ulong seed)
    {
        var info = new PermuteMeta(spawner);
        var state = new SpawnState(spawner.BaseCount, info.MaxAlive);

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
        var (empty, respawn, ghosts) = state.GetRespawnInfo();
        var (reseed, aggro) = GenerateSpawns(meta, table, isBonus, seed, empty, ghosts);

        // Update spawn state
        var newState = state.Generate(respawn, aggro);
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
        for (int i = 1; i <= state.MaxCountBattle; i++)
        {
            var step = (int)Advance.A1 + (i - 1);
            meta.Start((Advance)step);
            var newState = state.Knockout(i);
            PermuteRecursion(meta, table, isBonus, seed, newState);
            meta.End();
        }

        // If we can scare multiple, try this route too
        for (int i = 2; i <= state.MaxCountScare; i++)
        {
            var step = (int)Advance.S2 + (i - 2);
            meta.Start((Advance)step);
            var newState = state.Scare(i);
            PermuteRecursion(meta, table, isBonus, seed, newState);
            meta.End();
        }
    }

    private static (ulong Seed, int Aggressive) GenerateSpawns(PermuteMeta spawn, in ulong table, in bool isBonus, in ulong seed, int count, in int ghosts)
    {
        int aggressive = 0;
        var rng = new Xoroshiro128Plus(seed);
        for (int i = 1; i <= count; i++)
        {
            var subSeed = rng.Next();
            _ = rng.Next(); // Unknown

            if (i <= ghosts)
                continue; // end of wave ghost -- ghosts spawn first!

            var generate = SpawnGenerator.Generate(subSeed, table, spawn.Spawner.Type);
            if (spawn.IsResult(generate))
                spawn.AddResult(generate, i, isBonus);

            if (generate.IsAggressive)
                aggressive++;
        }
        var result = rng.Next(); // Reset the seed for future spawns.
        return (result, aggressive);
    }

    private static void PermuteFinish(PermuteMeta meta, in ulong table, in ulong seed, in SpawnState state)
    {
        PermuteBonusTable(meta, seed);

        // Try adding ghost spawns if we haven't capped out yet.
        if (state.CanAddGhosts)
            PermuteAddGhosts(meta, seed, table, state);
    }

    private static void PermuteBonusTable(PermuteMeta meta, in ulong seed)
    {
        meta.Start(Advance.CR);
        var state = new SpawnState(meta.Spawner.BonusCount, meta.MaxAlive);
        PermuteOutbreak(meta, meta.Spawner.BonusTable, true, seed, state);
        meta.End();
    }

    private static void PermuteAddGhosts(PermuteMeta meta, in ulong seed, in ulong table, in SpawnState state)
    {
        var remain = state.EmptyGhostSlots;
        for (int i = 1; i <= remain; i++)
        {
            var step = (int)Advance.G1 + (i - 1);
            meta.Start((Advance)step);
            var newState = state.AddGhosts(i);
            var gSeed = Calculations.GetGroupSeed(seed, newState.Ghost);
            PermuteRecursion(meta, table, false, gSeed, newState);
            meta.End();
        }
    }
}
