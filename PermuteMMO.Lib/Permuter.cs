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
    public static PermuteMeta Permute(SpawnInfo spawner, in ulong seed, int maxDepth = 15)
    {
        var info = new PermuteMeta(spawner, maxDepth);
        var state = spawner.GetStartingState();

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
        PermuteNextTable(meta, next, seed, state);

        // Try adding ghost spawns if we haven't capped out yet.
        if (meta.Spawner.AllowGhosts && state.CanAddGhosts)
            PermuteAddGhosts(meta, seed, table, state);

        // Outbreak complete.
    }

    private static void PermuteOutbreak(PermuteMeta meta, in ulong table, in ulong seed, in SpawnState state)
    {
        // Re-spawn to capacity
        var (reseed, newState) = UpdateRespawn(meta, table, seed, state);
        ContinuePermute(meta, table, reseed, newState);
    }

    public static (ulong, SpawnState) UpdateRespawn(PermuteMeta meta, ulong table, ulong seed, SpawnState state)
    {
        if (state.Count == 0)
            return (seed, state);
        var (empty, respawn, ghosts) = state.GetRespawnInfo();
        var (reseed, alpha, aggro, beta, oblivious)
            = GenerateSpawns(meta, table, seed, empty, ghosts, state.AliveAlpha, meta.Spawner.NoMultiAlpha);

        // Update spawn state
        var newState = state.Add(respawn, alpha, aggro, beta, oblivious);
        return (reseed, newState);
    }

    private static void ContinuePermute(PermuteMeta meta, in ulong table, in ulong seed, in SpawnState state)
    {
        var spawner = meta.Spawner;
        // If our spawner loops (regular), handle differently.
        if (spawner.Type is SpawnType.Regular)
        {
            // Try KO none if a future state can spawn more.
            var countSeed = spawner.Count.CountSeed;
            if (spawner.Count.CanSpawnMore(state.Alive))
            {
                meta.Start(Advance.RG);
                PermuteRecursion(meta, table, seed, state);
                meta.End();
                spawner.Count.CountSeed = countSeed;
            }

            // Try all Knockout->Respawn steps.
            for (int i = 1; i <= state.Alive; i++)
            {
                var step = (int)Advance.A1 + (i - 1);
                meta.Start((Advance)step);
                var newState = state.KnockoutAny(i);
                PermuteRecursion(meta, table, seed, newState);
                meta.End();
                spawner.Count.CountSeed = countSeed;
            }
            return;
        }

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

    private static (ulong Seed, int Alpha, int Aggressive, int Skittish, int Oblivious)
        GenerateSpawns(PermuteMeta meta, in ulong table, in ulong seed, int count, in int ghosts, int currentAlpha, bool onlyOneAlpha)
    {
        int alpha = 0;
        int aggressive = 0;
        int beta = 0;
        int oblivious = 0;
        var rng = new Xoroshiro128Plus(seed);
        for (int i = 1; i <= count; i++)
        {
            var subSeed = rng.Next(); // generate/slot seed
            var alphaSeed = rng.Next(); // alpha move, don't care

            if (i <= ghosts)
                continue; // end of wave ghost -- ghosts spawn first!

            bool noAlpha = onlyOneAlpha && currentAlpha + alpha != 0;
            var generate = SpawnGenerator.Generate(seed, i, subSeed, alphaSeed, table, meta.Spawner.Type, noAlpha);
            if (generate is null) // only a consideration for spawners with 100% static alphas, qty >1, maybe some weather/time tables?
                continue; // empty ghost slot -- spawn failure.

            if (meta.IsResult(generate))
                meta.AddResult(generate);

            if (generate.IsAlpha) alpha++;
            else if (generate.IsOblivious) oblivious++;
            else if (generate.IsSkittish) beta++;
            else aggressive++;
        }
        var result = rng.Next(); // Reset the seed for future spawns.
        return (result, alpha, aggressive + alpha, beta, oblivious);
    }

    private static void PermuteNextTable(PermuteMeta meta, SpawnInfo next, in ulong seed, in SpawnState exist)
    {
        if (!next.RetainExisting)
            meta.Start(Advance.CR);

        var current = meta.Spawner;
        meta.Spawner = next;
        var newAlive = next.Count.GetNextCount();

        SpawnState state;
        if (next.RetainExisting)
        {
            state = exist.AdjustCount(newAlive);
        }
        else
        {
            state = next.GetStartingState();
        }
        PermuteOutbreak(meta, next.Set.Table, seed, state);

        meta.Spawner = current;
        if (!next.RetainExisting)
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
