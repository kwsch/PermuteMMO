﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using PKHeX.Core;

namespace PermuteMMO.Reversal;

public static class IterativeReversal
{
    private const string LibraryPath = "PLA-SeedFinder";

    static IterativeReversal()
    {
        const string dllPath = LibraryPath + ".dll";
        if (!File.Exists(dllPath))
            File.WriteAllBytes(dllPath, Properties.Resources.PLA_SeedFinder);
    }

    [DllImport(LibraryPath, CallingConvention = CallingConvention.Cdecl)]
    private static extern int pa_PLA_find_seeds(uint pid, uint ec, ref int ivs, byte max_rolls, ref ulong seeds, ref byte rolls, int length);

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
    public static int FindSeeds(uint pid, uint ec, ReadOnlySpan<int> ivs, byte max_rolls, Span<ulong> seeds, Span<byte> rolls)
    {
        Debug.Assert(seeds.Length == rolls.Length);
        var length = seeds.Length;

        var ptrIVs = MemoryMarshal.GetReference(ivs);
        var ptrSeeds = MemoryMarshal.GetReference(seeds);
        var ptrRoll = MemoryMarshal.GetReference(rolls);
        return pa_PLA_find_seeds(pid, ec, ref ptrIVs, max_rolls, ref ptrSeeds, ref ptrRoll, length);
    }

    /// <inheritdoc cref="FindSeeds(uint,uint,ReadOnlySpan{int},byte,Span{ulong},Span{byte})"/>
    public static int FindSeeds(PKM pk, byte max_rolls, Span<ulong> seeds, Span<byte> rolls)
    {
        ReadOnlySpan<int> ivs = stackalloc int[6]
        {
            pk.IV_HP,
            pk.IV_ATK,
            pk.IV_DEF,
            pk.IV_SPA,
            pk.IV_SPD,
            pk.IV_SPE,
        };
        return FindSeeds(pk.PID, pk.EncryptionConstant, ivs, max_rolls, seeds, rolls);
    }

    public static ulong[] GetSeeds(uint pid, uint ec, Span<int> ivs, byte max_rolls)
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
