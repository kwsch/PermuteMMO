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

    public SpawnInfo GetSpawn()
    {
        var table = Parse(BaseTable);
        var bonus = Parse(BonusTable);
        if (table < 1000)
            table = Species;

        return new SpawnInfo
        {
            BaseCount = BaseCount,
            BonusCount = BonusCount,
            BaseTable = table,
            BonusTable = bonus,
            Type = bonus is 0 && table is 0 ? SpawnType.Outbreak : SpawnType.MMO,
        };
    }

    private static ulong Parse(string hex)
    {
        if (hex.StartsWith("0x"))
            hex = hex[2..];
        return ulong.Parse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
    }
}
