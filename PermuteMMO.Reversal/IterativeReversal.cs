using System.Diagnostics;
using System.Runtime.InteropServices;
using PKHeX.Core;

namespace PermuteMMO.Reversal;

public static class IterativeReversal
{
    private const string LibraryPath = "PLA-SeedFinder";

    static IterativeReversal()
    {
        const string dllPath = LibraryPath + ".dll";
        if (File.Exists(dllPath))
            return;

        var bin = Environment.Is64BitProcess
            ? Properties.Resources.PLA_SeedFinder_64
            : Properties.Resources.PLA_SeedFinder_32;
        File.WriteAllBytes(dllPath, bin);
    }

    [DllImport(LibraryPath, CallingConvention = CallingConvention.Cdecl)]
    private static extern int pa_PLA_find_seeds(uint pid, uint ec, ref byte ivs, byte max_rolls, ref ulong seeds, ref byte rolls, int length);

    /// <summary>
    /// Finds all seeds using the Pokémon Automation's reversal algorithm (parallelized brute forcing)
    /// </summary>
    /// <param name="pid">Entity PID</param>
    /// <param name="ec">Entity Encryption Constant</param>
    /// <param name="ivs">Entity IVs</param>
    /// <param name="max_rolls">Maximum amount of shiny rolls permitted</param>
    /// <param name="seeds">Result storage for all seeds</param>
    /// <param name="rolls">Result storage for amount of shiny rolls performed prior to the seed attempt stopping</param>
    /// <returns>Count of seed-rolls stored in the input spans.</returns>
    public static int FindSeeds(uint pid, uint ec, ReadOnlySpan<byte> ivs, byte max_rolls, Span<ulong> seeds, Span<byte> rolls)
    {
        Debug.Assert(seeds.Length == rolls.Length);
        var length = seeds.Length;

        return pa_PLA_find_seeds(pid, ec, ref MemoryMarshal.GetReference(ivs), max_rolls, ref MemoryMarshal.GetReference(seeds), ref MemoryMarshal.GetReference(rolls), length);
    }

    /// <inheritdoc cref="FindSeeds(uint,uint,ReadOnlySpan{byte},byte,Span{ulong},Span{byte})"/>
    public static int FindSeeds(PKM pk, byte max_rolls, Span<ulong> seeds, Span<byte> rolls)
    {
        ReadOnlySpan<byte> ivs = stackalloc byte[6]
        {
            (byte)pk.IV_HP,
            (byte)pk.IV_ATK,
            (byte)pk.IV_DEF,
            (byte)pk.IV_SPA,
            (byte)pk.IV_SPD,
            (byte)pk.IV_SPE,
        };
        return FindSeeds(pk.PID, pk.EncryptionConstant, ivs, max_rolls, seeds, rolls);
    }

    public static ulong[] GetSeeds(uint pid, uint ec, Span<byte> ivs, byte max_rolls)
    {
        const int overkill = 0x10; // normally 0-2 results, but let's go overboard :)
        Span<ulong> possible = stackalloc ulong[overkill];
        Span<byte> rolls = stackalloc byte[overkill];

        int count = FindSeeds(pid, ec, ivs, max_rolls, possible, rolls);
        Debug.Assert(count <= overkill);

        return possible[..count].ToArray();
    }

    public static ulong[] GetSeeds(PKM pk, byte max_rolls)
    {
        const int overkill = 0x10; // normally 0-2 results, but let's go overboard :)
        Span<ulong> possible = stackalloc ulong[overkill];
        Span<byte> rolls = stackalloc byte[overkill];

        int count = FindSeeds(pk, max_rolls, possible, rolls);
        Debug.Assert(count <= overkill);

        return possible[..count].ToArray();
    }
}
