namespace PermuteMMO.Lib;

/// <summary>
/// Type of encounter to generate (determining PID re-roll count)
/// </summary>
public enum SpawnType
{
    Regular  = 7 + 0,
    MMO      = 7 + 12,
    Outbreak = 7 + 25,
}
