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
    public static PermuteMeta Permute(SpawnInfo spawner, in ulong seed, int maxDepth = 50)
    {
        var info = new PermuteMeta(spawner, maxDepth);
        var state = new SpawnState(spawner.Set.Count, spawner.Detail.MaxAlive);

        // Generate the encounters!
        PermuteRecursion(info, spawner.Set.Table, seed, state);
        return info;
    }

    private static void PermuteRecursion(PermuteMeta meta, in ulong table, in ulong seed, in SpawnState state)
    {
        // If the outbreak is not done, continue.
        if (state.Count != 0)
        {
            PermuteOutbreak(meta, table, seed, state);
            return;
        }

        var (canContinue, next) = meta.AttemptNextWave();
        if (!canContinue)
            return;

        // Try the next table before we try adding ghosts.
        PermuteNextTable(meta, next, seed);

        // Try adding ghost spawns if we haven't capped out yet.
        if (state.CanAddGhosts)
            PermuteAddGhosts(meta, seed, table, state);

        // Outbreak complete.
    }

    private static void PermuteOutbreak(PermuteMeta meta, in ulong table, in ulong seed, in SpawnState state)
    {
        // Re-spawn to capacity
        var (empty, respawn, ghosts) = state.GetRespawnInfo();
        var (reseed, aggro) = GenerateSpawns(meta, table, seed, empty, ghosts);

        // Update spawn state
        var newState = state.Generate(respawn, aggro);
        ContinuePermute(meta, table, reseed, newState);
    }

    private static void ContinuePermute(PermuteMeta meta, in ulong table, in ulong seed, in SpawnState state)
    {
        // Check if we're now out of possible re-spawns
        if (state.Count == 0)
        {
            PermuteRecursion(meta, table, seed, state);
            return;
        }

        // Permute our remaining options
        for (int i = 1; i <= state.MaxCountBattle; i++)
        {
            var step = (int)Advance.A1 + (i - 1);
            meta.Start((Advance)step);
            var newState = state.Knockout(i);
            PermuteRecursion(meta, table, seed, newState);
            meta.End();
        }

        // If we can scare multiple, try this route too
        for (int i = 2; i <= state.MaxCountScare; i++)
        {
            var step = (int)Advance.S2 + (i - 2);
            meta.Start((Advance)step);
            var newState = state.Scare(i);
            PermuteRecursion(meta, table, seed, newState);
            meta.End();
        }
    }

    private static (ulong Seed, int Aggressive) GenerateSpawns(PermuteMeta meta, in ulong table, in ulong seed, int count, in int ghosts)
    {
        int aggressive = 0;
        var rng = new Xoroshiro128Plus(seed);
        for (int i = 1; i <= count; i++)
        {
            var subSeed = rng.Next();
            _ = rng.Next(); // Unknown

            if (i <= ghosts)
                continue; // end of wave ghost -- ghosts spawn first!

            var generate = SpawnGenerator.Generate(subSeed, table, meta.Spawner.Detail.SpawnType);
            if (meta.IsResult(generate))
                meta.AddResult(generate, i);

            if (generate.IsAggressive)
                aggressive++;
        }
        var result = rng.Next(); // Reset the seed for future spawns.
        return (result, aggressive);
    }

    private static void PermuteNextTable(PermuteMeta meta, SpawnInfo next, in ulong seed)
    {
        meta.Start(Advance.CR);
        var current = meta.Spawner;
        meta.Spawner = next;

        var state = new SpawnState(next.Set.Count, next.Detail.MaxAlive);
        PermuteOutbreak(meta, next.Set.Table, seed, state);

        meta.Spawner = current;
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
            PermuteRecursion(meta, table, gSeed, newState);
            meta.End();
        }
    }
}
