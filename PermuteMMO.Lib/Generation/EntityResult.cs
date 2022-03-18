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

    public bool IsShiny { get; set; }
    public bool IsAlpha { get; set; }
    public byte Height { get; set; }
    public byte Weight { get; set; }

    public string GetSummary()
    {
        var shiny = IsShiny ? $" {RollCount,2} {(ShinyXor == 0 ? '■' : '*')}(^{ShinyXor,2})" : "";
        var alpha = IsAlpha ? "α-" : "";
        var notAlpha = !IsAlpha ? " -- NOT ALPHA" : "";
        var gender = Gender == 2 ? "" : Gender == 1 ? " (F)" : " (M)";
        return $"{alpha}{Name}{gender}:{shiny}{notAlpha}";
    }
}
