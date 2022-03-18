using System.Runtime.CompilerServices;
using PermuteMMO.Lib.Properties;
using PKHeX.Core;

namespace PermuteMMO.Lib;

/// <summary>
/// Generator logic for creating new <see cref="EntityResult"/> objects.
/// </summary>
public static class SpawnGenerator
{
    public static readonly IReadOnlyDictionary<ulong, SlotDetail[]> EncounterTables = JsonDecoder.GetDictionary(Resources.mmo_es);

    #region Public Mutable - Useful for DLL consumers
    public static SAV8LA SaveFile { get; set; } = GetFake();
    public static PokedexSave8a Pokedex => SaveFile.PokedexSave;
    public static byte[] BackingArray => SaveFile.Blocks.GetBlock(0x02168706).Data;
    public static bool HasCharm { get; set; } = true;
    #endregion

    private static SAV8LA GetFake()
    {
        if (File.Exists("main"))
        {
            var data = File.ReadAllBytes("main");
            var sav = new SAV8LA(data);
            HasCharm = sav.Inventory.Any(z => z.Items.Any(i => i.Index == 632 && i.Count is not 0));
            return sav;
        }
        return new SAV8LA();
    }

    /// <summary>
    /// Generates an <see cref="EntityResult"/> from the input <see cref="seed"/> and <see cref="table"/>.
    /// </summary>
    public static EntityResult Generate(in ulong seed, in ulong table, SpawnType type)
    {
        var slotrng = new Xoroshiro128Plus(seed);

        var slots = EncounterTables[table];
        var slotSum = slots.Sum(z => z.Rate);
        var slotroll = slotrng.NextFloat(slotSum);
        var slot = GetSlot(slots, slotroll);
        var genseed = slotrng.Next();

        // Determine stuff from slot detail
        var gt = PersonalTable.LA.GetFormEntry(slot.Species, slot.Form).Gender;

        // Get roll count from save file
        int shinyRolls = GetRerollCount(slot.Species, type);

        var result = GeneratePokemon(genseed, shinyRolls, slot.FlawlessIVs, gt, slot.IsAlpha);
        result.Level = GetLevel(slot, slotrng);
        result.IsAlpha = slot.IsAlpha;
        result.Seed = seed;
        result.Name = slot.Name;
        return result;
    }

    private static int GetRerollCount(in int species, SpawnType type)
    {
        bool perfect = Pokedex.IsPerfect(species);
        bool complete = Pokedex.IsComplete(species);
        return 1 + (complete ? 1 : 0) + (perfect ? 2 : 0) + (HasCharm ? 3 : 0) + (int)type;
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

    private static SlotDetail GetSlot(IEnumerable<SlotDetail> slots, float slotroll)
    {
        foreach (var s in slots)
        {
            slotroll -= s.Rate;
            if (slotroll <= 0)
                return s;
        }
        throw new ArgumentOutOfRangeException(nameof(slotroll));
    }

    public static EntityResult GeneratePokemon(in ulong seed, in int shinyrolls, in int flawless, in int genderRatio, in bool isAlpha)
    {
        var rng = new Xoroshiro128Plus(seed);
        var result = new EntityResult();

        // Encryption Constant
        var encryptionConstant = (uint)rng.NextInt();
        result.EC = encryptionConstant;

        // Fake TID
        var fakeTID = (uint)rng.NextInt();
        result.FakeTID = fakeTID;

        // PID
        uint pid;
        int ctr = 0;
        do
        {
            ++ctr;
            pid = (uint)rng.NextInt();
            var ShinyXor = GetShinyXor(pid, fakeTID);
            var isShiny = result.IsShiny = ShinyXor < 16;
            if (!isShiny)
                continue;

            result.RollCount = ctr;
            result.ShinyXor = ShinyXor;
            result.PermittedRolls = shinyrolls;
            break;
        } while (ctr < shinyrolls);
        result.PID = pid;

        const int UNSET = -1;
        int[] ivs = { UNSET, UNSET, UNSET, UNSET, UNSET, UNSET };
        const int MAX = 31;
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
                ivs[i] = (int)rng.NextInt(32);
        }

        var ability = (int)rng.NextInt(2);

        int gender = genderRatio switch
        {
            PersonalInfo.RatioMagicGenderless => 2,
            PersonalInfo.RatioMagicFemale => 1,
            PersonalInfo.RatioMagicMale => 0,
            _ => (int)rng.NextInt(252) + 1 < genderRatio ? 1 : 0,
        };

        int nature = (int)rng.NextInt(25);

        var (height, weight) = isAlpha
            ? (byte.MaxValue, byte.MaxValue)
            : ((byte)((int)rng.NextInt(0x81) + (int)rng.NextInt(0x80)),
               (byte)((int)rng.NextInt(0x81) + (int)rng.NextInt(0x80)));

        result.Ability = ability;
        result.Gender = gender;
        result.Nature = nature;
        result.Height = height;
        result.Weight = weight;

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint GetShinyXor(in uint pid, in uint oid)
    {
        var xor = pid ^ oid;
        return (xor ^ (xor >> 16)) & 0xFFFF;
    }
}