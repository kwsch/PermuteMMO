namespace PermuteMMO.Lib;

/// <summary>
/// Stores object-type references for cleaner passing internally. Only refer to <see cref="Results"/> when done.
/// </summary>
public sealed record PermuteMeta(SpawnInfo Spawner)
{
    /// <summary>
    /// Global configuration for determining if a <see cref="EntityResult"/> is a suitable result.
    /// </summary>
    public static Func<EntityResult, IReadOnlyList<Advance>, bool> SatisfyCriteria { private get; set; } = (result, _) => result.IsShiny && result.IsAlpha;

    public readonly List<PermuteResult> Results = new();
    private readonly List<Advance> Advances = new();

    /// <summary>
    /// Signals the start of a recursive permutation step.
    /// </summary>
    /// <param name="adv">Step taken</param>
    public void Start(Advance adv) => Advances.Add(adv); // add to end

    /// <summary>
    /// Signals the end of a recursive permutation step.
    /// </summary>
    public void End() => Advances.RemoveAt(Advances.Count - 1); // pop off end

    /// <summary>
    /// Stores a result.
    /// </summary>
    public void AddResult(EntityResult entity, in int index, in bool isBonus)
    {
        var steps = Advances.ToArray();
        var result = new PermuteResult(steps, entity, index, isBonus);
        Results.Add(result);
    }

    /// <summary>
    /// Checks if the <see cref="entity"/> is a suitable result.
    /// </summary>
    public bool IsResult(EntityResult entity) => SatisfyCriteria(entity, Advances);

    /// <summary>
    /// Calls <see cref="PermuteResult.Print"/> for all objects in the result list.
    /// </summary>
    public void PrintResults(bool indicateSkittish)
    {
        foreach (var result in Results)
            result.Print(indicateSkittish);
    }

    public bool HasResults => Results.Count is not 0;
}

public sealed record PermuteResult(Advance[] Advances, EntityResult Entity, in int SpawnIndex, in bool IsBonus)
{
    public void Print(bool skittishBase, bool skittishBonus = false)
    {
        var steps = string.Join("|", Advances.Select(z => z.GetName()));
        // 37 total characters for the steps:
        // 10+7 spawner has 6+(3)+3=12 max permutations, +"SB|", remove last |; (3*12+2)=37.
        Console.WriteLine($"* {steps,-37} >>> {(IsBonus ? "Bonus " : "")}Spawn{SpawnIndex} = {Entity.GetSummary(Advances, skittishBase, skittishBonus)}");
    }
}
