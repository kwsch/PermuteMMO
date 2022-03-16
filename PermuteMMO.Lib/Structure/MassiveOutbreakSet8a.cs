namespace PermuteMMO.Lib;

/// <summary>
/// Overall block data for Massive Mass Outbreaks, containing all areas and their spawners.
/// </summary>
public sealed class MassiveOutbreakSet8a
{
    public const int AreaCount = 5;

    private readonly byte[] Data;

    public MassiveOutbreakSet8a(byte[] data) => Data = data;

    public MassiveOutbreakArea8a this[int index] => new(Data.AsSpan(MassiveOutbreakArea8a.SIZE * index, MassiveOutbreakArea8a.SIZE));
}
