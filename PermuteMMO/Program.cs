using PermuteMMO.Lib;

PermuteMeta.SatisfyCriteria = result => result.IsShiny;
var data = File.ReadAllBytes("mmo.bin");
ConsolePermuter.PermuteBlock(data);
