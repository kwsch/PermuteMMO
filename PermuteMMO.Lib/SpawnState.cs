using System.Diagnostics;

namespace PermuteMMO.Lib;

/// <summary>
/// Models the state of an Entity Spawner, indicating how it can be mutated by player actions.
/// </summary>
/// <param name="Count">Total count of entities that can be spawned by the spawner.</param>
/// <param name="MaxAlive">Maximum count of entities that can be alive at a given time.</param>
/// <param name="Alive">Current count of entities alive.</param>
/// <param name="Ghost">Current count of fake entities.</param>
/// <param name="AliveAggressive">Current count of aggressive entities alive.</param>
public readonly record struct SpawnState(in int Count, in int MaxAlive, in int Alive = 0, in int Ghost = 0, in int AliveAggressive = 0)
{
    /// <summary> Current count of unpopulated entities. </summary>
    public int Dead { get; init; } = MaxAlive;

    /// <summary> Total count of entities that can exist as ghosts. </summary>
    /// <remarks> Completely filling with ghost slots will start the next wave rather than add ghosts. </remarks>
    private int MaxGhosts => MaxAlive - 1;
    /// <summary> Current count of timid entities alive. </summary>
    public int AliveTimid => Alive - AliveAggressive;

    /// <summary> Maximum count of entities that can be battled in the current state. </summary>
    public int MaxCountBattle => Math.Min(Alive, AliveAggressive + 1);

    /// <summary> Maximum count of entities that can be scared in the current state. </summary>
    public int MaxCountScare => Math.Min(Alive, AliveTimid);

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
    /// If <see cref="count"/> is 1, this is the same as capturing a single Entity out of battle.
    /// </remarks>
    public SpawnState Knockout(in int count)
    {
        // Prefer to knock out the Skittish, and any required Aggressives
        var newAggro = AliveAggressive - count + 1;
        Debug.Assert(newAggro >= 0);
        return this with { Alive = Alive - count, Dead = Dead + count, AliveAggressive = newAggro };
    }

    /// <summary>
    /// Returns a spawner state after scaring existing entities away.
    /// </summary>
    public SpawnState Scare(in int count)
    {
        // Can only scare Skittish
        Debug.Assert(AliveTimid >= count);
        return this with { Alive = Alive - count, Dead = Dead + count };
    }

    /// <summary>
    /// Returns a spawner state after generating new entities.
    /// </summary>
    public SpawnState Generate(in int count, in int aggro) => this with
    {
        Count = Count - count,
        Alive = Alive + count,
        Dead = Dead - count,
        Ghost = Dead - count,
        AliveAggressive = AliveAggressive + aggro,
    };

    /// <summary>
    /// Returns a spawner state with additional ghosts added.
    /// </summary>
    public SpawnState AddGhosts(in int count) => this with
    {
        Alive = Alive - count,
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
