using System.Diagnostics;
using static PermuteMMO.Lib.Advance;

namespace PermuteMMO.Lib;

/// <summary>
/// Advancement step labels.
/// </summary>
public enum Advance : byte
{
    RG,
    CR,

    A1, A2, A3, A4, // Aggressive
    B1, B2, B3, B4, // Beta

    O1, O2, O3, O4, // Oblivious

 // S1 is equivalent to B1
        S2, S3, S4,

    // G4 is equivalent to CR
    G1, G2, G3,
}

public static class AdvanceRemoval
{
    public static (int Aggro, int Beta, int Oblivious) GetRemovals(this Advance advance)
    {
        var count = advance.AdvanceCount();
        if (advance.IsMultiAggressive() || advance is A1)
            return (count, 0, 0);
        if (advance.IsMultiBeta() || advance is B1)
            return (count - 1, 1, 0);
        if (advance.IsMultiScare())
            return (0, count, 0);
        if (advance.IsMultiOblivious() || advance is O1)
            return (count - 1, 0, 1);

        throw new ArgumentOutOfRangeException(nameof(advance));
    }

    public static SpawnState AdvanceState(this Advance advance, SpawnState state)
    {
        var (aggro, beta, oblivious) = advance.GetRemovals();
        return state.Remove(aggro, beta, oblivious);
    }

    [DebuggerDisplay($"{{{nameof(StepSummary)},nq}}")]
    public readonly record struct SpawnStep(Advance Step, SpawnState State, ulong Seed, ulong CountSeed)
    {
        public string StepSummary => $"{Step} {State.State} {State.Count} {Seed:X16} {CountSeed:X16}";
    }

    public static IReadOnlyList<SpawnStep> RunForwards(PermuteMeta meta, Advance[] advances, ulong seed)
    {
        List<SpawnStep> steps = new();
        var spawner = meta.Spawner;
        var state = spawner.GetStartingState();
        (seed, state) = Permuter.UpdateRespawn(meta, meta.Spawner.Set.Table, seed, state);
        steps.Add(new(RG, state, seed, meta.Spawner.Count.CountSeed));
        foreach (var adv in advances)
        {
            meta.Start(adv);
            if (meta.Spawner.RetainExisting)
            {
                var count = adv.AdvanceCount();
                if (count != 0)
                    state = state.KnockoutAny(count);

                var newAlive = meta.Spawner.Count.GetNextCount();
                state = state.AdjustCount(newAlive);
                steps.Add(new(adv, state, seed, meta.Spawner.Count.CountSeed));
            }
            else if (adv == CR)
            {
                if (!meta.Spawner.GetNextWave(out var next))
                    throw new ArgumentException(nameof(adv));
                meta.Spawner = next;
                state = next.GetStartingState();
                steps.Add(new(adv, state, seed, meta.Spawner.Count.CountSeed));
            }
            else if (adv >= G1)
            {
                var count = adv.AdvanceCount();
                state = state.AddGhosts(count);
                seed = Calculations.GetGroupSeed(seed, state.Ghost);
                steps.Add(new(adv, state, seed, meta.Spawner.Count.CountSeed));
                continue;
            }
            else
            {
                state = adv.AdvanceState(state);
                steps.Add(new(adv, state, seed, meta.Spawner.Count.CountSeed));
            }

            if (state.Count != 0)
                (seed, state) = Permuter.UpdateRespawn(meta, meta.Spawner.Set.Table, seed, state);
            steps.Add(new(RG, state, seed, meta.Spawner.Count.CountSeed));
        }

        return steps;
    }
}

public static class AdvanceExtensions
{
    /// <summary>
    /// Option to just emit the <see cref="Advance.ToString()"/> result instead of a humanized string.
    /// </summary>
    public static bool Raw { get; set; } = true;

    /// <summary>
    /// Returns a string for indicating the value of the <see cref="advance"/> step.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">If undefined</exception>
    public static string GetName(this Advance advance) => Raw ? advance.ToString() : Humanize(advance);

    private static string Humanize(Advance advance) => advance switch
    {
        CR => "Clear Remaining",

        A1 => "1 Aggressive",
        A2 => "2 Aggressive",
        A3 => "3 Aggressive",
        A4 => "4 Aggressive",

        B1 => "1 Beta",
        B2 => "1 Beta + 1 Aggressive",
        B3 => "1 Beta + 2 Aggressive",
        B4 => "1 Beta + 3 Aggressive",

        O1 => "1 Oblivious",
        O2 => "1 Oblivious + 1 Aggressive",
        O3 => "1 Oblivious + 2 Aggressive",
        O4 => "1 Oblivious + 3 Aggressive",

        G1 => "De-spawn 1 + Leave",
        G2 => "De-spawn 2 + Leave",
        G3 => "De-spawn 3 + Leave",

        S2 => "Multi Scare 2 + Leave",
        S3 => "Multi Scare 3 + Leave",
        S4 => "Multi Scare 4 + Leave",
        _ => throw new ArgumentOutOfRangeException(nameof(advance), advance, null)
    };

    /// <summary>
    /// Gets the count of advances required.
    /// </summary>
    public static int AdvanceCount(this Advance advance) => advance switch
    {
        A1 or B1 or O1       or G1 => 1,
        A2 or B2 or O1 or S2 or G2 => 2,
        A3 or B3 or O1 or S3 or G3 => 3,
        A4 or B4 or O1 or S4       => 4,
        _ => 0,
    };

    /// <summary>
    /// Indicates if a multi-battle is required for this advancement.
    /// </summary>
    public static bool IsMultiAny(this Advance advance) => advance.IsMultiAggressive() || advance.IsMultiBeta() || advance.IsMultiScare() || advance.IsMultiOblivious();

    /// <summary>
    /// Indicates if a multi-battle is required for this advancement.
    /// </summary>
    public static bool IsMultiAggressive(this Advance advance) => advance is (A2 or A3 or A4);

    /// <summary>
    /// Indicates if a multi-battle is required for this advancement.
    /// </summary>
    public static bool IsMultiScare(this Advance advance) => advance is (S2 or S3 or S4);

    /// <summary>
    /// Indicates if a multi-battle is required for this advancement.
    /// </summary>
    public static bool IsMultiBeta(this Advance advance) => advance is (B2 or B3 or B4);

    /// <summary>
    /// Indicates if a multi-battle is required for this advancement.
    /// </summary>
    public static bool IsMultiOblivious(this Advance advance) => advance is (O2 or O3 or O4);

    public static bool IsAny<T>(this ReadOnlySpan<T> span, Func<T, bool> check)
    {
        foreach (var x in span)
        {
            if (check(x))
                return true;
        }
        return false;
    }
}
