namespace PermuteMMO.Lib;

/// <summary>
/// <see cref="EntityResult"/> wrapper with some utility logic to print to console.
/// </summary>
public sealed record PermuteResult(Advance[] Advances, EntityResult Entity, in int SpawnIndex)
{
    private bool IsBonus => Array.IndexOf(Advances, Advance.CR) != -1;
    private int WaveIndex => Advances.Count(adv => adv == Advance.CR);

    public string GetLine(PermuteResult? prev, bool isActionMultiResult, bool hasChildChain)
    {
        var steps = GetSteps(prev);
        var feasibility = GetFeasibility(Advances);
        // 37 total characters for the steps:
        // 10+7 spawner has 6+(3)+3=12 max permutations, +"CR|", remove last |; (3*12+2)=37.
        var line = $"* {steps,-37} >>> {GetWaveIndicator()}Spawn{SpawnIndex} = {Entity.GetSummary()}{feasibility}";
        if (prev != null || hasChildChain)
            line += " ~~ Chain result!";
        if (isActionMultiResult)
            line += " ~~ Spawns multiple results!";
        return line;
    }

    private string GetWaveIndicator()
    {
        if (!IsBonus)
            return "      ";
        var waveIndex = WaveIndex;
        if (waveIndex == 1)
            return "Bonus ";
        return    $"Wave {waveIndex}";
    }

    public string GetSteps(PermuteResult? prev = null)
    {
        var steps = string.Join("|", Advances.Select(z => z.GetName()));
        if (prev is not { } p)
            return steps;

        var prevSeq = p.GetSteps();
        return string.Concat(Enumerable.Repeat("-> ", (prevSeq.Length+2)/3)) + steps[(prevSeq.Length + 1)..];
    }

    private static string GetFeasibility(ReadOnlySpan<Advance> advances)
    {
        if (advances.IsAny(AdvanceExtensions.IsMultiScare))
        {
            if (advances.IsAny(AdvanceExtensions.IsMultiBeta))
                return " -- Skittish: Multi scaring with aggressive!";
            return " -- Skittish: Multi scaring!";
        }
        if (advances.IsAny(AdvanceExtensions.IsMultiBeta))
            return " -- Skittish: Aggressive!";

        if (advances.IsAny(z => z == Advance.B1))
        {
            if (!advances.IsAny(AdvanceExtensions.IsMultiAggressive))
                return " -- Skittish: Single advances!";
            return " -- Skittish: Mostly aggressive!";
        }

        if (advances.IsAny(AdvanceExtensions.IsMultiAggressive))
            return string.Empty;

        return " -- Single advances!";
    }
}
