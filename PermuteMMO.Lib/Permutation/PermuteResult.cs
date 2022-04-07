namespace PermuteMMO.Lib;

/// <summary>
/// <see cref="EntityResult"/> wrapper with some utility logic to print to console.
/// </summary>
public sealed record PermuteResult(Advance[] Advances, EntityResult Entity, in int SpawnIndex, in bool IsBonus)
{
    public string GetLine(PermuteResult? prev, bool skittishBase, bool skittishBonus)
    {
        var steps = GetSteps(prev);
        // 37 total characters for the steps:
        // 10+7 spawner has 6+(3)+3=12 max permutations, +"SB|", remove last |; (3*12+2)=37.
        var line = $"* {steps,-37} >>> {(IsBonus ? "Bonus " : "      ")}Spawn{SpawnIndex} = {Entity.GetSummary(Advances, skittishBase, skittishBonus)}";
        if (prev != null)
            line += " ~~ Chain result!";
        return line;
    }

    public string GetSteps(PermuteResult? prev = null)
    {
        var steps = string.Join("|", Advances.Select(z => z.GetName()));
        if (prev is not { } p)
            return steps;

        var prevSeq = p.GetSteps();
        return string.Concat(Enumerable.Repeat("-> ", (prevSeq.Length+2)/3)) + steps[(prevSeq.Length + 1)..];
    }
}
