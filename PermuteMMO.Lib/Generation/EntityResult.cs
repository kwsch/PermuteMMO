using PKHeX.Core;

namespace PermuteMMO.Lib;

/// <summary>
/// Spawned Pokémon Data that can be encountered.
/// </summary>
public sealed class EntityResult
{
    public string Name { get; init; } = string.Empty;
    public readonly byte[] IVs = { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue };

    public ulong Seed { get; init; }
    public int Level { get; init; }

    public uint EC { get; set; }
    public uint FakeTID { get; set; }
    public uint PID { get; set; }

    public uint ShinyXor { get; set; }
    public int RollCountUsed { get; set; }
    public int RollCountAllowed { get; set; }
    public ushort Species { get; init; }
    public ushort Form { get; init; }

    public bool IsShiny { get; set; }
    public bool IsAlpha { get; init; }
    public byte Ability { get; set; }
    public byte Gender { get; set; }
    public byte Nature { get; set; }
    public byte Height { get; set; }
    public byte Weight { get; set; }

    public bool IsTimid => BehaviorUtil.Timid.Contains(Species);

    public string GetSummary(ushort species, ReadOnlySpan<Advance> advances)
    {
        var shiny = IsShiny ? $" {RollCountUsed,2} {(ShinyXor == 0 ? '■' : '*')}" : "";
        var ivs = $" {IVs[0]:00}/{IVs[1]:00}/{IVs[2]:00}/{IVs[3]:00}/{IVs[4]:00}/{IVs[5]:00}";
        var nature = $" {GameInfo.GetStrings(1).Natures[Nature]}";
        var alpha = IsAlpha ? "α-" : "";
        var notAlpha = !IsAlpha ? " -- NOT ALPHA" : "";
        var gender = Gender switch
        {
            2 => "",
            1 => " (F)",
            _ => " (M)",
        };
        var timid = GetTimidString(species, advances);
        return $"{alpha}{Name}{gender}:{shiny}{ivs}{nature,-8}{notAlpha}{timid}";
    }

    private string GetTimidString(ushort species, ReadOnlySpan<Advance> advances)
    {
        var baseTimid = BehaviorUtil.Timid.Contains(species);
        if (!baseTimid)
            return string.Empty;

        var anyMulti = advances.IsAnyMulti();
        if (anyMulti)
            return " -- Timid, multi :(";

        if (IsTimid)
            return " -- TIMID, NOT MULTI";
        return " -- Base encounter Timid, NOT MULTI.";
    }
}
