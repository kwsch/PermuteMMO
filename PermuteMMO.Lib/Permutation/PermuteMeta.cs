namespace PermuteMMO.Lib;

/// <summary>
/// Stores object-type references for cleaner passing internally. Only refer to <see cref="Results"/> when done.
/// </summary>
public sealed record PermuteMeta(SpawnInfo Spawner, int MaxDepth)
{
    /// <summary>
    /// Global configuration for determining if a <see cref="EntityResult"/> is a suitable result.
    /// </summary>
    public static Func<EntityResult, IReadOnlyList<Advance>, bool> SatisfyCriteria { get; set; } = (result, _) => result.IsShiny && result.IsAlpha;

    public Func<EntityResult, IReadOnlyList<Advance>, bool> Criteria { get; set; } = SatisfyCriteria;

    public readonly List<PermuteResult> Results = [];
    private readonly List<Advance> Advances = new(MaxDepth);

    public PermuteMeta Copy() => new(Spawner, MaxDepth);

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
    public void AddResult(EntityResult entity)
    {
        var steps = Advances.ToArray();
        var result = new PermuteResult(steps, entity);
        Results.Add(result);
    }

    /// <summary>
    /// Checks if the <see cref="entity"/> is a suitable result.
    /// </summary>
    public bool IsResult(EntityResult entity) => Criteria(entity, Advances);

    /// <summary>
    /// Calls <see cref="PermuteResult.GetLine"/> for all objects in the result list.
    /// </summary>
    public IEnumerable<string> GetLines()
    {
        for (var i = 0; i < Results.Count; i++)
        {
            var result = Results[i];
            var parent = FindNearestParentAdvanceResult(i, result.Advances);
            bool isActionMultiResult = IsActionMultiResult(i, result.Advances);
            bool hasChildChain = HasChildChain(i, result.Advances);
            yield return result.GetLine(parent, isActionMultiResult, hasChildChain);
        }
    }

    private bool HasChildChain(int index, ReadOnlySpan<Advance> parent)
    {
        if (++index >= Results.Count)
            return false;
        return IsSubset(parent, Results[index].Advances);
    }

    private bool IsActionMultiResult(int index, ReadOnlySpan<Advance> child)
    {
        int count = 0;
        // scan backwards until different
        for (int i = index - 1; i >= 0; i--)
        {
            if (child.SequenceEqual(Results[i].Advances))
                count++;
            else
                break;
        }
        // scan forwards until different
        for (int i = index + 1; i < Results.Count; i++)
        {
            if (child.SequenceEqual(Results[i].Advances))
                count++;
            else
                break;
        }
        return count != 0;
    }

    // Non-null indicates this is a chain of results the user might want to pick (compared to other results).
    private PermuteResult? FindNearestParentAdvanceResult(int index, ReadOnlySpan<Advance> child)
    {
        var start = index - 1;
        if (start < 0)
            return null;

        // Due to how we depth-first search, previous results can contain overlapping advancement sequences.
        // Find nearest previous result with advancement sequence being a subset of our child's sequence.
        for (var i = start; i >= 0; i--)
        {
            if (IsSubset(Results[i].Advances, child))
                return Results[i];
        }
        return null;
    }

    private static bool IsSubset(ReadOnlySpan<Advance> parent, ReadOnlySpan<Advance> child)
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
