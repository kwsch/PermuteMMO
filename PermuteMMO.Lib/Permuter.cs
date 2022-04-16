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
        var (reseed, aggro, beta, oblivious) = GenerateSpawns(meta, table, seed, empty, ghosts);

        // Update spawn state
        var newState = state.Generate(respawn, aggro, beta, oblivious);
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

        // Depending on what spawns in future calls, the actions we take here can impact the options for future recursion.
        // We need to try out every single potential action the player can do, and target removals for specific behaviors.

        // De-spawn: Aggressive Only
        if (state.AliveAggressive != 0)
        {
            for (int i = 1; i <= state.AliveAggressive; i++)
            {
                var step = (int)Advance.A1 + (i - 1);
                meta.Start((Advance)step);
                var newState = state.KnockoutAggressive(i);
                PermuteRecursion(meta, table, seed, newState);
                meta.End();
            }
        }

        if (state.AliveOblivious != 0)
        {
            for (int i = 0; i <= state.AliveAggressive; i++)
            {
                var step = (int)Advance.O1 + i;
                meta.Start((Advance)step);
                var newState = state.KnockoutOblivious(i + 1);
                PermuteRecursion(meta, table, seed, newState);
                meta.End();
            }
        }

        // De-spawn: Single beta with aggressive(s) / none.
        if (state.AliveBeta != 0)
        {
            for (int i = 0; i <= state.AliveAggressive; i++)
            {
                var step = (int)Advance.B1 + i;
                meta.Start((Advance)step);
                var newState = state.KnockoutBeta(i + 1);
                PermuteRecursion(meta, table, seed, newState);
                meta.End();
            }
        }

        // De-spawn: Multiple betas (Scaring)
        for (int i = 2; i <= state.AliveBeta; i++)
        {
            var step = (int)Advance.S2 + (i - 2);
            meta.Start((Advance)step);
            var newState = state.Scare(i);
            PermuteRecursion(meta, table, seed, newState);
            meta.End();
        }
    }

    private static (ulong Seed, int Aggressive, int Skittish, int Oblivious) GenerateSpawns(PermuteMeta meta, in ulong table, in ulong seed, int count, in int ghosts)
    {
        int aggressive = 0;
        int beta = 0;
        int oblivious = 0;
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

            if (generate.IsAlpha) aggressive++;
            else if (generate.IsOblivious) oblivious++;
            else if (generate.IsSkittish) beta++;
            else aggressive++;
        }
        var result = rng.Next(); // Reset the seed for future spawns.
        return (result, aggressive, beta, oblivious);
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
