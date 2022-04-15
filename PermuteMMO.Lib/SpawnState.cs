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
public readonly record struct SpawnState(in int Count, in int MaxAlive, in int Ghost = 0, in int AliveAggressive = 0, in int AliveBeta = 0, in int AliveOblivious = 0)
{
    /// <summary> Current count of unpopulated entities. </summary>
    public int Dead { get; init; } = MaxAlive;

    /// <summary> Total count of entities that can exist as ghosts. </summary>
    /// <remarks> Completely filling with ghost slots will start the next wave rather than add ghosts. </remarks>
    private int MaxGhosts => MaxAlive - 1;

    /// <summary> Indicates if ghost entities can be added to the spawner. </summary>
    /// <remarks> Only call this if <see cref="Count"/> is zero. </remarks>
    public bool CanAddGhosts => Ghost != MaxGhosts;

    /// <summary> Indicates if ghost entities can be added to the spawner. </summary>
    /// <remarks> Only call this if <see cref="Count"/> is zero. </remarks>
    public int EmptyGhostSlots => MaxGhosts - Ghost;

    /// <summary>
    /// Returns a spawner state after knocking out existing entities.
    /// </summary>
    /// <remarks>
    /// If <see cref="count"/> is 1, this is the same as capturing a single Aggressive Entity out of battle.
    /// </remarks>
    public SpawnState KnockoutAggressive(in int count)
    {
        // Knock out required Aggressive
        var newAggro = AliveAggressive - count;
        Debug.Assert(newAggro >= 0);
        return this with { Dead = Dead + count, AliveAggressive = newAggro };
    }

    /// <summary>
    /// Returns a spawner state after knocking out existing entities.
    /// </summary>
    /// <remarks>
    /// If <see cref="count"/> is 1, this is the same as capturing a single Beta Entity out of battle.
    /// </remarks>
    public SpawnState KnockoutBeta(in int count)
    {
        // Prefer to knock out the Skittish, and any required Aggressive
        var newAggro = AliveAggressive - count + 1;
        Debug.Assert(newAggro >= 0);
        return this with { Dead = Dead + count, AliveAggressive = newAggro, AliveBeta = AliveBeta - 1 };
    }

    /// <summary>
    /// Returns a spawner state after knocking out existing entities.
    /// </summary>
    public SpawnState KnockoutOblivious(int count)
    {
        // Knock out required Aggressive
        var newOblivious = AliveOblivious - 1;
        var newAggro = AliveAggressive - count + 1;
        Debug.Assert(newOblivious >= 0);
        Debug.Assert(newAggro >= 0);
        return this with { Dead = Dead + 1, AliveOblivious = newOblivious };
    }

    /// <summary>
    /// Returns a spawner state after scaring existing Beta entities away.
    /// </summary>
    public SpawnState Scare(in int count)
    {
        // Can only scare Skittish
        Debug.Assert(AliveBeta >= count);
        return this with { AliveBeta = AliveBeta - count, Dead = Dead + count };
    }

    /// <summary>
    /// Returns a spawner state after generating new entities.
    /// </summary>
    public SpawnState Generate(in int count, in int aggro, in int beta, in int oblivious) => this with
    {
        Count = Count - count,
        Dead = Dead - count,
        Ghost = Dead - count,
        AliveAggressive = AliveAggressive + aggro,
        AliveBeta = AliveBeta + beta,
        AliveOblivious = AliveOblivious + oblivious,
    };

    /// <summary>
    /// Returns a spawner state with additional ghosts added.
    /// </summary>
    public SpawnState AddGhosts(in int count) => this with
    {
        // These are no longer important, don't bother choosing which to decrement.
        // We only check Ghost count going forward.
        AliveAggressive = 0,
        AliveOblivious = 0,
        AliveBeta = 0,

        Dead = Dead + count,
        Ghost = Ghost + count,
    };

    /// <summary>
    /// Gets the counts of what to generate when regenerating a spawner.
    /// </summary>
    /// <remarks> Only call this if <see cref="Count"/> is NOT zero. </remarks>
    public (int Empty, int Respawn, int Ghosts) GetRespawnInfo()
    {
        var emptySlots = Dead;
        var respawn = Math.Min(Count, emptySlots);
        var ghosts = emptySlots == respawn ? 0 : MaxAlive - respawn;

        Debug.Assert(respawn != 0);
        return (emptySlots, respawn, ghosts);
    }
}
