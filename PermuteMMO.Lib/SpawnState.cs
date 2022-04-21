using System.Diagnostics;

namespace PermuteMMO.Lib;

/// <summary>
/// Models the state of an Entity Spawner, indicating how it can be mutated by player actions.
/// </summary>
/// <param name="Count">Total count of entities that can be spawned by the spawner.</param>
/// <param name="MaxAlive">Maximum count of entities that can be alive at a given time.</param>
/// <param name="Ghost">Current count of fake entities.</param>
/// <param name="AliveAggressive">Current count of aggressive entities alive.</param>
/// <param name="AliveBeta">Current count of timid entities alive.</param>
/// <param name="AliveOblivious">Current count of oblivious entities alive.</param>
[DebuggerDisplay($"{{{nameof(State)},nq}}")]
public readonly record struct SpawnState(in int Count, in int MaxAlive, in int Ghost = 0, in int AliveAlpha = 0, in int AliveAggressive = 0, in int AliveBeta = 0, in int AliveOblivious = 0)
{
    /// <summary> Current count of unpopulated entities. </summary>
    public int Dead { get; init; } = MaxAlive;

    public int Alive => MaxAlive - Dead;

    /// <summary> Total count of entities that can exist as ghosts. </summary>
    /// <remarks> Completely filling with ghost slots will start the next wave rather than add ghosts. </remarks>
    private int MaxGhosts => MaxAlive - 1;

    /// <summary> Indicates if ghost entities can be added to the spawner. </summary>
    /// <remarks> Only call this if <see cref="Count"/> is zero. </remarks>
    public bool CanAddGhosts => Ghost != MaxGhosts;

    /// <summary> Indicates if ghost entities can be added to the spawner. </summary>
    /// <remarks> Only call this if <see cref="Count"/> is zero. </remarks>
    public int EmptyGhostSlots => MaxGhosts - Ghost;

    public static SpawnState Get(int count) => Get(count, count);
    public static SpawnState Get(int totalCount, int aliveCount) => new(totalCount, aliveCount);

    /// <summary>
    /// Returns a spawner state after knocking out existing entities.
    /// </summary>
    /// <remarks>
    /// If <see cref="count"/> is 1, this is the same as capturing a single Aggressive Entity out of battle.
    /// </remarks>
    public SpawnState KnockoutAggressive(in int count) => Remove(aggro: count);

    /// <summary>
    /// Returns a spawner state after knocking out existing entities.
    /// </summary>
    /// <remarks>
    /// If <see cref="count"/> is 1, this is the same as capturing a single Beta Entity out of battle.
    /// </remarks>
    public SpawnState KnockoutBeta(in int count) => Remove(aggro: count - 1, beta: 1);

    /// <summary>
    /// Returns a spawner state after knocking out existing entities.
    /// </summary>
    public SpawnState KnockoutOblivious(int count) => Remove(aggro: count - 1, oblivious: 1);

    public SpawnState KnockoutAny(int count)
    {
        int aggro = Math.Max(0, Math.Min(AliveAggressive, count));
        int beta = Math.Max(0, Math.Min(AliveBeta - aggro, count));
        int obli = Math.Max(0, Math.Min(AliveOblivious - aggro - beta, count));
        return Remove(aggro, beta, obli);
    }

    /// <summary>
    /// Returns a spawner state after scaring existing Beta entities away.
    /// </summary>
    public SpawnState Scare(in int count) => Remove(beta: count);

    /// <summary>
    /// Returns a spawner state after generating new entities.
    /// </summary>
    public SpawnState Add(in int count, in int alpha, in int aggro, in int beta, in int oblivious)
    {
        var nAlpha = AliveAlpha + alpha;
        var nAggro = AliveAggressive + aggro;
        var nBeta = AliveBeta + beta;
        var nOblivious = AliveOblivious + oblivious;
        Debug.Assert((uint)nAlpha <= MaxAlive);
        Debug.Assert((uint)nAggro <= MaxAlive);
        Debug.Assert((uint)nBeta <= MaxAlive);
        Debug.Assert((uint)nOblivious <= MaxAlive);

        var delta = (aggro + beta + oblivious);
        var nDead = Dead - count;
        var nGhost = nDead;
        Debug.Assert(delta > 0);
        Debug.Assert(count >= delta);
        Debug.Assert((uint)nDead < Dead);
        Debug.Assert((uint)nGhost < MaxAlive);
        Debug.Assert(nGhost <= Dead);

        return this with
        {
            Count = Count - count,
            AliveAlpha = nAlpha,
            AliveAggressive = nAggro,
            AliveBeta = nBeta,
            AliveOblivious = nOblivious,
            Dead = nDead,
            Ghost = nGhost,
        };
    }

    public SpawnState Remove(in int aggro = 0, in int beta = 0, in int oblivious = 0)
    {
        // Any aggressive should prefer removing alphas for regular spawners to prevent instant despawns of future alphas.
        var nAlpha = AliveAlpha - Math.Min(AliveAlpha, aggro);
        var nAggro = AliveAggressive - aggro;
        var nBeta = AliveBeta - beta;
        var nOblivious = AliveOblivious - oblivious;
        Debug.Assert((uint)nAlpha <= MaxAlive);
        Debug.Assert((uint)nAggro <= MaxAlive);
        Debug.Assert((uint)nBeta <= MaxAlive);
        Debug.Assert((uint)nOblivious <= MaxAlive);

        var delta = (aggro + beta + oblivious);
        var nDead = Dead + delta;
        Debug.Assert((uint)nDead <= MaxAlive);
        Debug.Assert(delta > 0);

        return this with
        {
            AliveAlpha = nAlpha,
            AliveAggressive = nAggro,
            AliveBeta = nBeta,
            AliveOblivious = nOblivious,
            Dead = nDead,
        };
    }

    public SpawnState AdjustCount(int newAlive)
    {
        var maxAlive = Math.Max(newAlive, Alive);
        var newCount = Math.Max(0, maxAlive - Alive);
        var newDead = maxAlive - Alive;
        return this with { MaxAlive = maxAlive, Count = newCount, Dead = newDead };
    }

    /// <summary>
    /// Returns a spawner state with additional ghosts added.
    /// </summary>
    /// <remarks> Don't care about the alive breakdown; reaching here we only care about the amount of ghosts. </remarks>
    public SpawnState AddGhosts(in int count) => new(Count, MaxAlive, Ghost + count) { Dead = Dead + count };

    /// <summary>
    /// Gets the counts of what to generate when regenerating a spawner.
    /// </summary>
    /// <remarks> Only call this if <see cref="Count"/> is NOT zero. </remarks>
    public (int Empty, int Respawn, int Ghosts) GetRespawnInfo()
    {
        var emptySlots = Dead;
        var respawn = Math.Min(Count, emptySlots);
        var ghosts = emptySlots - respawn;

        Debug.Assert(respawn != 0 || Dead == 0);
        return (emptySlots, respawn, ghosts);
    }

    public string State => GetState();

    private string GetState()
    {
        int ctr = 0;
        char[] result = new char[MaxAlive];
        for (int i = 0; i < AliveAlpha; i++)
            result[ctr++] = 'a';
        for (int i = 0; i < AliveAggressive - AliveAlpha; i++)
            result[ctr++] = 'A';
        for (int i = 0; i < AliveBeta; i++)
            result[ctr++] = 'B';
        for (int i = 0; i < AliveOblivious; i++)
            result[ctr++] = 'O';
        for (int i = 0; i < Ghost; i++)
            result[ctr++] = '~';
        for (int i = 0; i < Dead - Ghost; i++)
            result[ctr++] = 'X';
        while (ctr != result.Length)
            result[ctr++] = '?'; // shouldn't hit here besides ghosts
        return new string(result);
    }
}
