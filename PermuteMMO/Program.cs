using PermuteMMO.Lib;

// Change the criteria for emitting matches here.
PermuteMeta.SatisfyCriteria = (result, advances) => result.IsShiny;

// Load mmo file if present, otherwise fall back to whatever is in the save file.
const string file = "mmo.bin";
byte[] data;
if (File.Exists(file))
    data = File.ReadAllBytes(file);
else
    data = SpawnGenerator.SaveFile.Accessor.GetBlock(0x7799EB86).Data;

// Compute and print.
ConsolePermuter.PermuteBlock(data);
