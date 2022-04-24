using PKHeX.Core;

namespace PermuteMMO.Lib;

/// <summary>
/// Spawned Pokémon Data that can be encountered.
/// </summary>
public sealed record EntityResult(SlotDetail Slot)
{
    public readonly byte[] IVs = { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue };

    public ulong GroupSeed { get; init; }
    public int Index { get; init; }
    public ulong SlotSeed { get; init; }
    public float SlotRoll { get; init; }
    public ulong GenSeed { get; init; }
    public ulong AlphaSeed { get; init; }
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

    public bool IsOblivious => BehaviorUtil.Oblivious.Contains(Species);
    public bool IsSkittish => BehaviorUtil.Skittish.Contains(Species);
    public bool IsAggressive => IsAlpha || !(IsSkittish || IsOblivious);

    public string GetSummary()
    {
        var shiny = IsShiny ? $" {RollCountUsed,2} {(ShinyXor == 0 ? '■' : '*')}" : "";
        var ivs = $" {IVs[0]:00}/{IVs[1]:00}/{IVs[2]:00}/{IVs[3]:00}/{IVs[4]:00}/{IVs[5]:00}";
        var nature = $" {GameInfo.GetStrings(1).Natures[Nature]}";
        var alpha = IsAlpha ? "α-" : "  ";
        var notAlpha = !IsAlpha ? " -- NOT ALPHA" : "";
        var gender = Gender switch
        {
            2 => "",
            1 => " (F)",
            _ => " (M)",
        };
        return $"{alpha}{Slot.Name}{gender}:{shiny}{ivs}{nature,-8}{notAlpha}";
    }

    public IEnumerable<string> GetLines()
    {
        var shiny = IsShiny ? $" {RollCountUsed,2} {(ShinyXor == 0 ? '■' : '*')}" : "";
        var s = GameInfo.GetStrings(1);
        var alpha = IsAlpha ? "α-" : "";
        yield return shiny + alpha + Slot.Name;
        yield return $"Group Seed: {GroupSeed:X16}";
        yield return $"Alpha Move Seed: {AlphaSeed:X16}";
        yield return $"Slot Seed: {SlotSeed:X16}";
        yield return $"Slot: {SlotRoll:F5}";
        yield return $"Level: {Level}";
        yield return $"Seed: {GenSeed:X16}";
        yield return $"  EC: {EC:X8}";
        yield return $"  PID: {PID:X8}";
        yield return $"  Flawless IVs: {Slot.FlawlessIVs}";
        yield return $"  IVs: {string.Join('/', IVs)}";
        yield return $"  Ability: {Ability}";
        yield return $"  Gender: {Gender switch { 0 => "M", 1 => "F", _ => "-" }}";
        yield return $"  Nature: {s.Natures[Nature]}";
        yield return $"  {Height} | {Weight}";
    }
}
