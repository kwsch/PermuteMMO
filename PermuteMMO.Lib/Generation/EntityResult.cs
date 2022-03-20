namespace PermuteMMO.Lib;

/// <summary>
/// Spawned Pokémon Data that can be encountered.
/// </summary>
public sealed class EntityResult
{
    public ulong Seed { get; set; }
    public string Name { get; set; } = string.Empty;
    public uint EC { get; set; }
    public uint FakeTID { get; set; }

    public uint PID { get; set; }
    public int RollCount { get; set; }
    public uint ShinyXor { get; set; }
    public int Level { get; set; }
    public int PermittedRolls { get; set; }
    public int Ability { get; set; }
    public int Gender { get; set; }
    public int Nature { get; set; }
    public ushort Species { get; set; }

    public bool IsShiny { get; set; }
    public bool IsAlpha { get; set; }
    public byte Height { get; set; }
    public byte Weight { get; set; }

    public bool IsTimid => BehaviorUtil.Timid.Contains(Species);

    public string GetSummary(ushort species, IReadOnlyList<Advance> advances)
    {
        var shiny = IsShiny ? $" {RollCount,2} {(ShinyXor == 0 ? '■' : '*')}(^{ShinyXor,2})" : "";
        var alpha = IsAlpha ? "α-" : "";
        var notAlpha = !IsAlpha ? " -- NOT ALPHA" : "";
        var gender = Gender == 2 ? "" : Gender == 1 ? " (F)" : " (M)";
        var timid = GetTimidString(species, advances);
        return $"{alpha}{Name}{gender}:{shiny}{notAlpha}{timid}";
    }

    private string GetTimidString(ushort species, IEnumerable<Advance> readOnlyList)
    {
        var baseTimid = BehaviorUtil.Timid.Contains(species);
        if (!baseTimid)
            return string.Empty;

        var anyMulti = readOnlyList.IsAnyMulti();
        if (anyMulti)
            return " -- Timid, multi.";

        if (!IsTimid)
            return " -- Base Timid, not multi.";

        return "-- TIMID, NO MULTI";
    }
}
