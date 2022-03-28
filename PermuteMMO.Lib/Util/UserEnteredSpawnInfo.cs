using System.Globalization;

namespace PermuteMMO.Lib;

public sealed record UserEnteredSpawnInfo
{
    public ushort Species { get; set; }
    public string Seed { get; set; } = string.Empty;

    public ulong GetSeed() => ulong.Parse(Seed);

    public int BaseCount { get; set; }
    public string BaseTable { get; set; } = string.Empty;

    public int BonusCount { get; set; }
    public string BonusTable { get; set; } = string.Empty;

    public SpawnInfo GetSpawn() => new()
    {
        BaseCount = BaseCount,
        BonusCount = BonusCount,
        BaseTable = Parse(BaseTable),
        BonusTable = Parse(BonusTable),
        Type = Parse(BonusTable) is not 0 ? SpawnType.MMO : SpawnType.Outbreak,
    };

    private static ulong Parse(string hex)
    {
        if (hex.StartsWith("0x"))
            hex = hex[2..];
        return ulong.Parse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
    }
}
