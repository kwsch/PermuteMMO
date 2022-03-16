using PKHeX.Core;

namespace PermuteMMO.Lib;

/// <summary>
/// Logic to permute spawner data.
/// </summary>
public static class ConsolePermuter
{
    /// <summary>
    /// Permutes all the areas to print out all possible spawns.
    /// </summary>
    public static void PermuteBlock(byte[] data)
    {
        var block = new MassiveOutbreakSet8a(data);
        for (int i = 0; i < MassiveOutbreakSet8a.AreaCount; i++)
        {
            var area = block[i];
            var areaName = AreaUtil.AreaTable[area.AreaHash];
            if (!area.IsActive)
            {
                Console.WriteLine($"No outbreak in {areaName}.");
                continue;
            }

            Console.WriteLine($"Permuting all possible paths for {areaName}.");
            Console.WriteLine("==========");
            for (int j = 0; j < MassiveOutbreakArea8a.SpawnerCount; j++)
            {
                var spawner = area[j];
                var seed = spawner.SpawnSeed;
                var info = new SpawnInfo
                {
                    BaseCount = spawner.BaseCount,
                    BaseTable = spawner.BaseTable,

                    BonusCount = spawner.BonusCount,
                    BonusTable = spawner.BonusTable,
                };

                Permuter.Permute(info, seed);

                var result = Permuter.Permute(info, seed);
                if (!result.HasResults)
                    continue;

                Console.WriteLine($"Spawner {j+1} at ({spawner.X:F1}, {spawner.Y:F1}, {spawner.Z}) shows {SpeciesName.GetSpeciesName(spawner.DisplaySpecies, 2)}");
                Console.WriteLine($"Parameters: {info}");
                result.PrintResults();
                Console.WriteLine();
            }
            Console.WriteLine("Done permuting area.");
            Console.WriteLine("==========");
        }
    }

    public static void PermuteSingle(SpawnInfo spawn, ulong seed)
    {
        Console.WriteLine($"Permuting all possible paths for {seed:X16}.");
        Console.WriteLine($"Parameters: {spawn}");
        Console.WriteLine();

        var result = Permuter.Permute(spawn, seed);
        result.PrintResults();

        Console.WriteLine();
        Console.WriteLine("Done.");
    }
}
