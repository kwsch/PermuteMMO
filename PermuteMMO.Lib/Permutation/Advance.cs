using static PermuteMMO.Lib.Advance;

namespace PermuteMMO.Lib;

/// <summary>
/// Advancement step labels.
/// </summary>
public enum Advance : byte
{
    CR,

    A1,
    A2,
    A3,
    A4,

    G1,
    G2,
    G3,
 // G4 is equivalent to CR

 // S1 is equivalent to A1
    S2,
    S3,
    S4,
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

        A1 => "De-spawn 1",
        A2 => "Battle 2",
        A3 => "Battle 3",
        A4 => "Battle 4",

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
        A1       or G1 => 1,
        A2 or S2 or G2 => 2,
        A3 or S3 or G3 => 3,
        A4 or S4       => 4,
        _ => 0,
    };

    /// <summary>
    /// Indicates if a multi-battle is required for this advancement.
    /// </summary>
    public static bool IsMulti(this Advance advance) => advance is (A2 or A3 or A4);

    /// <summary>
    /// Indicates if a multi-battle is required for this advancement.
    /// </summary>
    public static bool IsScare(this Advance advance) => advance is (S2 or S3 or S4);

    /// <summary>
    /// Indicates if any advance requires a multi-battle for advancement.
    /// </summary>
    public static bool IsAnyMulti(this ReadOnlySpan<Advance> advances)
    {
        foreach (var adv in advances)
        {
            if (adv.IsMulti())
                return true;
        }

        return false;
    }

    /// <summary>
    /// Indicates if any advance requires a multi-scare for advancement.
    /// </summary>
    public static bool IsAnyMultiScare(this ReadOnlySpan<Advance> advances)
    {
        foreach (var adv in advances)
        {
            if (adv.IsScare())
                return true;
        }

        return false;
    }
}
