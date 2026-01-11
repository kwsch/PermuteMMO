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
    public static Memory<byte> BackingArray => SaveFile.Blocks.GetBlock(0x02168706).Raw;
    public static bool HasCharm { get; set; } = true;
    public static bool UseSaveFileShinyRolls { get; set; }

    public static Memory<byte> GetMassOutbreakData() => SaveFile.GetMassOutbreakData();
    public static Memory<byte> GetMassiveMassOutbreakData() => SaveFile.GetMassiveMassOutbreakData();

    extension(SAV8LA sav)
    {
        public Memory<byte> GetMassOutbreakData() => sav.Accessor.GetBlock(0x1E0F1BA3).Raw;
        public Memory<byte> GetMassiveMassOutbreakData() => sav.Accessor.GetBlock(0x7799EB86).Raw;
    }

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
        HasCharm = sav.Inventory.Any(z => z.Items.Any(IsShinyCharm));
        return sav;
    }

    private static bool IsShinyCharm(InventoryItem item) => item is { Index: 632, Count: not 0 };

    /// <summary>
    /// Gets the count of shiny rolls the player is permitted to have when rolling a <see cref="PKM.PID"/>.
    /// </summary>
    /// <param name="species">Encounter species</param>
    /// <param name="type">Encounter Spawn type</param>
    /// <returns>[1,X] iteration of PID rolls permitted</returns>
    public static int GetRerollCount(in ushort species, SpawnType type)
    {
        if (!UseSaveFileShinyRolls)
            return (int)type;
        bool perfect = Pokedex.IsPerfect(species);
        bool complete = Pokedex.IsComplete(species);
        return 1 + (complete ? 1 : 0) + (perfect ? 2 : 0) + (HasCharm ? 3 : 0) + (int)(type - 7);
    }
}
