using System.Buffers.Binary;

namespace PermuteMMO.Lib;

/// <summary>
/// Overall block data for Mass Outbreaks, containing all areas and their spawners.
/// </summary>
public readonly ref struct MassOutbreakSet8a
{
    public const int SIZE = 0x190;
    private readonly Span<byte> Data;
    public const int AreaCount = 5;

    public MassOutbreakSet8a(Span<byte> data) => Data = data;

    public MassOutbreakSpawner8a this[int index] => new(Data.Slice(MassOutbreakSpawner8a.SIZE * index, MassOutbreakSpawner8a.SIZE));
}
