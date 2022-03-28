using PKHeX.Core;

namespace PermuteMMO.Reversal;

internal static class GroupSeedFinder
{
    public const byte max_rolls = 32;

    public static IEnumerable<ulong> FindSeeds(string folder, byte maxRolls = max_rolls) => FindSeeds(Directory.EnumerateFiles(folder), maxRolls);
    public static IEnumerable<ulong> FindSeeds(IEnumerable<string> files, byte maxRolls = max_rolls) => FindSeeds(files.Select(File.ReadAllBytes), maxRolls);
    public static IEnumerable<ulong> FindSeeds(IEnumerable<byte[]> data, byte maxRolls = max_rolls) => FindSeeds(data.Select(PKMConverter.GetPKMfromBytes).OfType<PKM>(), maxRolls);

    public static IEnumerable<ulong> FindSeeds(IEnumerable<PKM> data, byte maxRolls = max_rolls)
    {
        var entities = data.ToArray();
        var ecs = entities.Select(z => z.EncryptionConstant).ToArray();
        var allPokeResults = entities.Select(z => IterativeReversal.GetSeeds(z, maxRolls));
        foreach (var pokeResult in allPokeResults)
        {
            foreach (var (pokeSeed, _) in pokeResult)
            {
                // Get seed for slot-pkm
                var genSeeds = GenSeedReversal.FindPotentialGenSeeds(pokeSeed);
                foreach (var genSeed in genSeeds)
                {
                    // Get the group seed
                    var groupSeeds = GenSeedReversal.FindPotentialGroupSeeds(genSeed);
                    foreach (var groupSeed in groupSeeds)
                    {
                        if (IsValidGroupSeed(groupSeed, ecs))
                            yield return groupSeed;
                    }
                }
            }
        }
    }

    private static bool IsValidGroupSeed(ulong groupSeed, uint[] ecs)
    {
        throw new NotImplementedException();
    }
}
