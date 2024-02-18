namespace PermuteMMO.Lib;

public static class AreaUtil
{
    private const string NONE = "(Empty Area Detail)";

    public static string GetAreaName(ulong areaHash) => areaHash switch
    {
        0 => NONE,
        0xCBF29CE484222645 => NONE,
        0xE3BBEF047A645A1D => "Obsidian Fieldlands",
        0xE3BBEC047A645504 => "Crimson Mirelands",
        0xE3BBED047A6456B7 => "Cobalt Coastlands",
        0xE3BBEA047A64519E => "Coronet Highlands",
        0xE3BBEB047A645351 => "Alabaster Icelands",
        _ => $"Unknown Area (0x{areaHash:X16})",
    };
}
