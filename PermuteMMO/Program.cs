using PermuteMMO.Lib;

PermuteMeta.SatisfyCriteria = (result, advances) => result.IsShiny;
var data = File.ReadAllBytes("mmo.bin");
ConsolePermuter.PermuteBlock(data);
