using System.Diagnostics;
using PKHeX.Core;

namespace PermuteMMO.Lib;

/// <summary>
/// Logic to permute spawner data.
/// </summary>
public static class ConsolePermuter
{
    static ConsolePermuter() => Console.OutputEncoding = System.Text.Encoding.Unicode;

    /// <summary>
    /// Permutes all the areas to print out all possible spawns.
    /// </summary>
    public static void PermuteMassiveMassOutbreak(Span<byte> data)
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
            Debug.Assert(area.IsValid);

            bool hasPrintedAreaMMO = false;
            for (int j = 0; j < MassiveOutbreakArea8a.SpawnerCount; j++)
            {
                var spawner = area[j];
                if (spawner.Status is MassiveOutbreakSpawnerStatus.None)
                    continue;

                Debug.Assert(spawner.HasBase);
                var seed = spawner.SpawnSeed;
                var spawn = new SpawnInfo(spawner);

                var result = Permuter.Permute(spawn, seed);
                if (!result.HasResults)
                    continue;

                if (!hasPrintedAreaMMO)
                {
                    Console.WriteLine($"Found paths for Massive Mass Outbreaks in {areaName}.");
                    Console.WriteLine("==========");
                    hasPrintedAreaMMO = true;
                }

                Console.WriteLine($"Spawner {j+1} at ({spawner.X:F1}, {spawner.Y:F1}, {spawner.Z}) shows {SpeciesName.GetSpeciesName(spawner.DisplaySpecies, 2)}");
                Console.WriteLine($"Parameters: {spawn}");
                Console.WriteLine($"Seed: {seed}");
                bool skittishBase = SpawnGenerator.IsSkittish(spawn.Set.Table);
                bool skittishBonus = spawn.GetNextWave(out var next) && SpawnGenerator.IsSkittish(next.Set.Table);
                var lines = result.GetLines(skittishBase, skittishBonus);
                foreach (var line in lines)
                    Console.WriteLine(line);
                Console.WriteLine();
            }

            if (!hasPrintedAreaMMO)
            {
                Console.WriteLine($"Found no results for any Massive Mass Outbreak in {areaName}.");
            }
            else
            {
                Console.WriteLine("Done permuting area.");
                Console.WriteLine("==========");
            }
        }
    }

    /// <summary>
    /// Permutes all the Mass Outbreaks to print out all possible spawns.
    /// </summary>
    public static void PermuteBlockMassOutbreak(Span<byte> data)
    {
        Console.WriteLine("Permuting Mass Outbreaks.");
        var block = new MassOutbreakSet8a(data);
        for (int i = 0; i < MassOutbreakSet8a.AreaCount; i++)
        {
            var spawner = block[i];
            var areaName = AreaUtil.AreaTable[spawner.AreaHash];
            if (!spawner.HasOutbreak)
            {
                Console.WriteLine($"No outbreak in {areaName}.");
                continue;
            }
            Debug.Assert(spawner.IsValid);

            var seed = spawner.SpawnSeed;
            var spawn = new SpawnInfo(spawner);
            var result = Permuter.Permute(spawn, seed);
            if (!result.HasResults)
            {
                Console.WriteLine($"Found no paths for {(Species)spawner.DisplaySpecies} Mass Outbreak in {areaName}.");
                continue;
            }

            Console.WriteLine($"Found paths for {(Species)spawner.DisplaySpecies} Mass Outbreak in {areaName}:");
            Console.WriteLine("==========");
            Console.WriteLine($"Spawner at ({spawner.X:F1}, {spawner.Y:F1}, {spawner.Z}) shows {SpeciesName.GetSpeciesName(spawner.DisplaySpecies, 2)}");
            Console.WriteLine($"Parameters: {spawn}");
            Console.WriteLine($"Seed: {seed}");
            bool skittishBase = SpawnGenerator.IsSkittish(spawner.DisplaySpecies);
            var lines = result.GetLines(skittishBase);
            foreach (var line in lines)
                Console.WriteLine(line);
            Console.WriteLine();
        }
        Console.WriteLine("Done permuting Mass Outbreaks.");
        Console.WriteLine("==========");
    }

    /// <summary>
    /// Permutes a single spawn with simple info.
    /// </summary>
    public static void PermuteSingle(SpawnInfo spawn, ulong seed, ushort species)
    {
        Console.WriteLine($"Permuting all possible paths for {seed:X16}.");
        Console.WriteLine($"Base Species: {SpeciesName.GetSpeciesName(species, 2)}");
        Console.WriteLine($"Parameters: {spawn}");
        Console.WriteLine($"Seed: {seed}");

        var result = Permuter.Permute(spawn, seed);
        if (!result.HasResults)
        {
            Console.WriteLine("No results found. Try another outbreak! :(");
        }
        else
        {
            bool skittishBase = SpawnGenerator.IsSkittish(spawn.Set.Table);
            bool skittishBonus = spawn.GetNextWave(out var next) && SpawnGenerator.IsSkittish(next.Set.Table);
            var lines = result.GetLines(skittishBase, skittishBonus);
            foreach (var line in lines)
                Console.WriteLine(line);
        }

        Console.WriteLine();
        Console.WriteLine("Done.");
    }
}
