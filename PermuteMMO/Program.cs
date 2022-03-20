using PermuteMMO.Lib;

// Change the criteria for emitting matches here.
PermuteMeta.SatisfyCriteria = (result, advances) => result.IsShiny;

const string file = "combo.bin";
Span<byte> data_mo, data_mmo;
if (File.Exists(file))
{
    Span<byte> data = File.ReadAllBytes(file);
    data_mo = data[..MassOutbreakSet8a.SIZE];
    data_mmo = data.Slice(MassOutbreakSet8a.SIZE, 0x3980);
}
else
{
    const string file_mo = "mo.bin";
    if (File.Exists(file))
        data_mo = File.ReadAllBytes(file_mo);
    else
        data_mo = SpawnGenerator.SaveFile.Accessor.GetBlock(0x1E0F1BA3).Data;

    const string file_mmo = "mmo.bin";
    if (File.Exists(file_mmo))
        data_mmo = File.ReadAllBytes(file_mmo);
    else
        data_mmo = SpawnGenerator.SaveFile.Accessor.GetBlock(0x7799EB86).Data;
}

// Compute and print.
ConsolePermuter.PermuteMassiveMassOutbreak(data_mmo);
Console.WriteLine();
Console.WriteLine("==========");
ConsolePermuter.PermuteBlockMassOutbreak(data_mo);
