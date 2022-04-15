using PKHeX.Core;

namespace PermuteMMO.Lib;

/// <summary>
/// Fetches environment specific values necessary for spawn generation.
/// </summary>
public static class SaveFileParameter
{
    #region Public Mutable - Useful for DLL consumers

    public static SAV8LA SaveFile { get; set; } = GetFake();
    public static PokedexSave8a Pokedex => SaveFile.PokedexSave;
    public static byte[] BackingArray => SaveFile.Blocks.GetBlock(0x02168706).Data;
    public static bool HasCharm { get; set; } = true;
    public static bool UseSaveFileShinyRolls { get; set; }

    public static byte[] GetMassOutbreakData() => SaveFile.GetMassOutbreakData();
    public static byte[] GetMassiveMassOutbreakData() => SaveFile.GetMassiveMassOutbreakData();

    public static byte[] GetMassOutbreakData(this SAV8LA sav) => sav.Accessor.GetBlock(0x1E0F1BA3).Data;
    public static byte[] GetMassiveMassOutbreakData(this SAV8LA sav) => sav.Accessor.GetBlock(0x7799EB86).Data;

    #endregion

    private static SAV8LA GetFake()
    {
        var mainPath = AppDomain.CurrentDomain.BaseDirectory;
        mainPath = Path.Combine(mainPath, "main");
        if (File.Exists(mainPath))
            return GetFromFile(mainPath);
        return new SAV8LA();
    }

    private static SAV8LA GetFromFile(string mainPath)
    {
        var data = File.ReadAllBytes(mainPath);
        var sav = new SAV8LA(data);
        UseSaveFileShinyRolls = true;
        HasCharm = sav.Inventory.Any(z => z.Items.Any(i => i.Index == 632 && i.Count is not 0));
        return sav;
    }

    /// <summary>
    /// Gets the count of shiny rolls the player is permitted to have when rolling an <see cref="PKM.PID"/>.
    /// </summary>
    /// <param name="species">Encounter species</param>
    /// <param name="type">Encounter Spawn type</param>
    /// <returns>[1,X] iteration of PID rolls permitted</returns>
    public static int GetRerollCount(in int species, SpawnType type)
    {
        if (!UseSaveFileShinyRolls)
            return (int)type;
        bool perfect = Pokedex.IsPerfect(species);
        bool complete = Pokedex.IsComplete(species);
        return 1 + (complete ? 1 : 0) + (perfect ? 2 : 0) + (HasCharm ? 3 : 0) + (int)(type - 7);
    }
}
