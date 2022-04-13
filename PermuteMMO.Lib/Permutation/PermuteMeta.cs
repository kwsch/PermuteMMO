namespace PermuteMMO.Lib;

/// <summary>
/// Stores object-type references for cleaner passing internally. Only refer to <see cref="Results"/> when done.
/// </summary>
public sealed record PermuteMeta(SpawnInfo Spawner, int MaxDepth)
{
    /// <summary>
    /// Global configuration for determining if a <see cref="EntityResult"/> is a suitable result.
    /// </summary>
    public static Func<EntityResult, IReadOnlyList<Advance>, bool> SatisfyCriteria { private get; set; } = (result, _) => result.IsShiny && result.IsAlpha;

    public readonly List<PermuteResult> Results = new();
    private readonly List<Advance> Advances = new();

    public bool HasResults => Results.Count is not 0;
    public SpawnInfo Spawner { get; set; } = Spawner;

    public (bool CanContinue, SpawnInfo Next) AttemptNextWave()
    {
        if (Advances.Count < MaxDepth && Spawner.GetNextWave(out var next))
            return (true, next);
        return (false, Spawner);
    }

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
    public void AddResult(EntityResult entity, in int index)
    {
        var steps = Advances.ToArray();
        var result = new PermuteResult(steps, entity, index);
        Results.Add(result);
    }

    /// <summary>
    /// Checks if the <see cref="entity"/> is a suitable result.
    /// </summary>
    public bool IsResult(EntityResult entity) => SatisfyCriteria(entity, Advances);

    /// <summary>
    /// Calls <see cref="PermuteResult.GetLine"/> for all objects in the result list.
    /// </summary>
    public IEnumerable<string> GetLines(bool skittishBase, bool skittishBonus = false)
    {
        for (var i = 0; i < Results.Count; i++)
        {
            var result = Results[i];
            var parent = FindNearestParentAdvanceResult(i, result.Advances);
            bool isActionMultiResult = IsActionMultiResult(i, result.Advances);
            yield return result.GetLine(parent, isActionMultiResult, skittishBase, skittishBonus);
        }
    }

    private bool IsActionMultiResult(int index, Advance[] child)
    {
        int count = 0;
        // scan backwards until different
        for (int i = index - 1; i >= 0; i--)
        {
            if (Results[i].Advances.SequenceEqual(child))
                count++;
            else
                break;
        }
        // scan forwards until different
        for (int i = index + 1; i < Results.Count; i++)
        {
            if (Results[i].Advances.SequenceEqual(child))
                count++;
            else
                break;
        }
        return count != 0;
    }

    private PermuteResult? FindNearestParentAdvanceResult(int index, Advance[] child)
    {
        var start = index - 1;
        if (start < 0)
            return null;

        // Due to how we depth-first search, previous results can contain overlapping advancement sequences.
        // Find nearest previous result with advancement sequence being a subset of our child's sequence.
        var nearest = Results.FindLastIndex(start, start, z => IsSubset(z.Advances, child));
        if (nearest == -1)
            return null;

        // Non-null indicates this is a chain of results the user might want to pick (compared to other results).
        return Results[nearest];
    }

    private static bool IsSubset(Advance[] parent, Advance[] child)
    {
        // check if parent sequence [0..n) matches child's [0..n)
        if (parent.Length >= child.Length)
            return false;
        for (var i = 0; i < parent.Length; i++)
        {
            if (parent[i] != child[i])
                return false;
        }
        return true;
    }
}
