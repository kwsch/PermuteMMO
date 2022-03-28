using PermuteMMO.Reversal;

const string entityFolderName = "mons";
var result = GroupSeedFinder.FindSeeds(entityFolderName).ToArray();
if (result.Length == 0)
{
    Console.WriteLine("No group seeds found with the input data. Double check your inputs.");
    return;
}

// Print seeds
Console.WriteLine($"Found {result.Length} {(result.Length == 1 ? "seed" : "seeds")}!");
foreach (var seed in result)
    Console.WriteLine(seed);
Console.WriteLine("Done!");

Console.WriteLine();
Console.WriteLine("Press [ENTER] to exit.");
