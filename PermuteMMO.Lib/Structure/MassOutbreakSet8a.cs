namespace PermuteMMO.Lib;

/// <summary>
/// Overall block data for Mass Outbreaks, containing all areas and their spawner objects.
/// </summary>
public readonly ref struct MassOutbreakSet8a(Span<byte> data)
{
    public const int SIZE = 0x190;
    public const int AreaCount = 5;

    private readonly Span<byte> Data = data;

    public MassOutbreakSpawner8a this[int index] => new(Data.Slice(MassOutbreakSpawner8a.SIZE * index, MassOutbreakSpawner8a.SIZE));
}
