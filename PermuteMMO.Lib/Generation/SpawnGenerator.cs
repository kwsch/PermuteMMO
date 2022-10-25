using System.Runtime.CompilerServices;
using PermuteMMO.Lib.Properties;
using PKHeX.Core;

namespace PermuteMMO.Lib;

/// <summary>
/// Generator logic for creating new <see cref="EntityResult"/> objects.
/// </summary>
public static class SpawnGenerator
{
    public static readonly IDictionary<ulong, SlotDetail[]> EncounterTables = JsonDecoder.GetDictionary(Resources.mmo_es);

    /// <summary>
    /// Generates an <see cref="EntityResult"/> from the input <see cref="seed"/> and <see cref="table"/>.
    /// </summary>
    public static EntityResult? Generate(in ulong groupseed, in int index, in ulong seed, in ulong alphaSeed, in ulong table, SpawnType type, bool noAlpha)
    {
        var slotrng = new Xoroshiro128Plus(seed);

        var slots = GetSlots(table);
        var slotSum = GetSlotSum(slots, noAlpha);
        if (slotSum == 0)
            return null;

        var slotroll = slotrng.NextFloat(slotSum);
        var slot = GetSlot(slots, slotroll, noAlpha);
        var genseed = slotrng.Next();
        var level = GetLevel(slot, slotrng);

        // Determine stuff from slot detail
        var gt = PersonalTable.LA.GetFormEntry(slot.Species, slot.Form).Gender;

        // Get roll count from save file
        int shinyRolls = SaveFileParameter.GetRerollCount(slot.Species, type);

        var result = new EntityResult(slot)
        {
            Species = slot.Species,
            Form = slot.Form,
            Level = level,
            IsAlpha = slot.IsAlpha,

            GroupSeed = groupseed,
            Index = index,
            SlotSeed = seed,
            GenSeed = genseed,

            AlphaSeed = alphaSeed,
            SlotRoll = slotroll,
        };

        GeneratePokemon(result, genseed, shinyRolls, slot.FlawlessIVs, gt);
        return result;
    }

    private static readonly Dictionary<ushort, SlotDetail[]> Outbreaks = new();
    private static readonly int[] FakeLevels = { 0, 1, 2 };

    private static SlotDetail[] GetSlots(in ulong table)
    {
        if (table > 1000)
            return EncounterTables[table];

        ushort species = (ushort)table;
        return GetFakeOutbreak(species);
    }

    private static SlotDetail[] GetFakeOutbreak(ushort species)
    {
        if (Outbreaks.TryGetValue(species, out var value))
            return value;

        var name = SpeciesName.GetSpeciesName(species, 2);
        if (species == (ushort)Species.Basculin)
            name = $"{name}-2";
        value = new[]
        {
            new SlotDetail(100, name, false, FakeLevels, 0),
            new SlotDetail(001, name, true, FakeLevels, 3),
        };
        foreach (var slot in value)
            slot.SetSpecies();

        return Outbreaks[species] = value;
    }

    private static int GetLevel(SlotDetail slot, Xoroshiro128Plus slotrng)
    {
        var min = slot.LevelMin;
        var max = slot.LevelMax;
        var level = min;
        var delta = max - min;
        if (delta != 0)
            level += (int)slotrng.NextInt((ulong)delta + 1);
        return level;
    }

    private static float GetSlotSum(IEnumerable<SlotDetail> slots, bool noAlpha)
    {
        float total = 0;
        foreach (var slot in slots)
        {
            if (noAlpha && slot.IsAlpha)
                continue;
            total += slot.Rate;
        }
        return total;
    }

    private static SlotDetail GetSlot(IEnumerable<SlotDetail> slots, float slotroll, bool noAlpha)
    {
        foreach (var slot in slots)
        {
            if (noAlpha && slot.IsAlpha)
                continue;

            slotroll -= slot.Rate;
            if (slotroll <= 0)
                return slot;
        }
        throw new ArgumentOutOfRangeException(nameof(slotroll));
    }

    public static void GeneratePokemon(EntityResult result, in ulong seed, in int shinyrolls, in int flawless, in int genderRatio)
    {
        var rng = new Xoroshiro128Plus(seed);

        // Encryption Constant
        result.EC = (uint)rng.NextInt();
        result.FakeTID = (uint)rng.NextInt();

        // PID
        uint pid;
        int ctr = 0;
        do
        {
            ++ctr;
            pid = (uint)rng.NextInt();
            var ShinyXor = GetShinyXor(pid, result.FakeTID);
            var isShiny = result.IsShiny = ShinyXor < 16;
            if (!isShiny)
                continue;

            result.ShinyXor = ShinyXor;
            result.RollCountUsed = ctr;
            result.RollCountAllowed = shinyrolls;
            break;
        } while (ctr < shinyrolls);
        result.PID = pid;

        const byte UNSET = byte.MaxValue;
        var ivs = result.IVs;
        const byte MAX = 31;
        for (int i = 0; i < flawless; i++)
        {
            int index;
            do { index = (int)rng.NextInt(6); }
            while (ivs[index] != UNSET);

            ivs[index] = MAX;
        }

        for (int i = 0; i < ivs.Length; i++)
        {
            if (ivs[i] == UNSET)
                ivs[i] = (byte)rng.NextInt(32);
        }
        result.Ability = (byte)rng.NextInt(2);
        result.Gender = genderRatio switch
        {
            PersonalInfo.RatioMagicGenderless => 2,
            PersonalInfo.RatioMagicFemale => 1,
            PersonalInfo.RatioMagicMale => 0,
            _ => (int)rng.NextInt(253) + 1 < genderRatio ? (byte)1: (byte)0,
        };
        result.Nature = (byte)rng.NextInt(25);

        (result.Height, result.Weight) = result.IsAlpha
            ? (byte.MaxValue, byte.MaxValue)
            : ((byte)((int)rng.NextInt(0x81) + (int)rng.NextInt(0x80)),
               (byte)((int)rng.NextInt(0x81) + (int)rng.NextInt(0x80)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint GetShinyXor(in uint pid, in uint oid)
    {
        var xor = pid ^ oid;
        return (xor ^ (xor >> 16)) & 0xFFFF;
    }
}
