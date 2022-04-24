using System.Linq;
using FluentAssertions;
using PermuteMMO.Lib;
using PKHeX.Core;
using Xunit;

namespace PermuteMMO.Tests;

/// <summary>
/// Aren't these nice to have? Examples??
/// </summary>
public sealed class MultiTests
{
    private static void SetFakeTable(SlotDetail[] slots, ulong key)
    {
        foreach (var s in slots)
            s.SetSpecies();

        SpawnGenerator.EncounterTables.Add(key, slots);
    }

    [Theory]
    [InlineData(0xB2204D9BA549D169u)]
    public void TestCombee22(in ulong seed)
    {
        const int count = 2; // 2-2 spawner

        const ulong key = 0x1337BABECAFEDEAD;
        var slots = new SlotDetail[]
        {
            new(100, "Combee", false, new [] {17, 20}, 0),
            new(2, "Combee", true , new [] {32, 35}, 3),
        };
        SetFakeTable(slots, key);

        var details = new SpawnCount(count, count);
        var set = new SpawnSet(key, count);
        var spawner = SpawnInfo.GetLoop(details, set, SpawnType.Regular);

        var results = Permuter.Permute(spawner, seed, 20);
        var min = results.Results
            .Where(z => z.Entity.Gender == 1 && z.Entity.RollCountUsed <= 5)
            .OrderBy(z => z.Advances.Length).FirstOrDefault();
        min.Should().NotBeNull();

        var seq = min!.Advances;
        var copy = results.Copy();
        var _ = AdvanceRemoval.RunForwards(copy, seq, seed);
        var expect = copy.Results.Where(z => seq.SequenceEqual(z.Advances));
        expect.FirstOrDefault(z => z.Entity.IsShiny).Should().NotBeNull();
    }

    [Theory]
    [InlineData(0x9C1107A569F7681D)]
    public void TestEevee(in ulong seed)
    {
        const int count = 2; // 2-2 spawner

        const ulong key = 0x1337BABE12345678;
        var slots = new SlotDetail[]
        {
            new(100, "Bidoof", false, new [] {3, 6}, 0),
            new(2, "Bidoof", true , new [] {17, 19}, 3),
            new(20, "Eevee", false, new [] {3, 6}, 0),
            new(1, "Eevee", true , new [] {17, 19}, 3),
        };
        SetFakeTable(slots, key);

        const int rolls = 5;
        static bool IsSatisfactory(PermuteResult z) => z.Entity.Species == (int)Species.Eevee && z.Entity.Gender == 1 && z.Entity.RollCountUsed <= rolls;

        var details = new SpawnCount(count, count);
        var set = new SpawnSet(key, count);
        var spawner = SpawnInfo.GetLoop(details, set, SpawnType.Regular);

        var results = Permuter.Permute(spawner, seed, 20);
        var min = results.Results
            .Where(IsSatisfactory)
            .OrderBy(z => z.Advances.Length).FirstOrDefault();
        min.Should().NotBeNull();

        var seq = min!.Advances;
        var copy = results.Copy();
        var _ = AdvanceRemoval.RunForwards(copy, seq, seed);
        var expect = copy.Results.Where(z => seq.SequenceEqual(z.Advances));
        expect.FirstOrDefault(z => z.Entity.IsShiny).Should().NotBeNull();
    }

    [Theory]
    [InlineData(0xA0B404668A34A9E6u, 0x6B9EBA2AD7437ADEu)]
    public void TestUnown23(in ulong seed, in ulong countSeed)
    {
        const int minCount = 2;
        const int maxCount = 3;

        SlotDetail[] slots = new SlotDetail[28 * 2];
        const ulong key = 9489890319879407414;
        for (int i = 0; i < slots.Length/2; i++)
        {
            var name = $"Unown{(i == 0 ? "" : $"-{i}")}";
            slots[i]      = new(100, name, false, new[] { 25, 25 }, 0);
            slots[i + 28] = new(001, name, true , new[] { 25, 25 }, 3);
        }
        SetFakeTable(slots, key);

        var details = new SpawnCount(maxCount, minCount, countSeed);
        var set = new SpawnSet(key, 0);
        var spawner = SpawnInfo.GetLoop(details, set, SpawnType.Regular);

        var results = Permuter.Permute(spawner, seed, 12);
        var min = results.Results
            .MinBy(z => z.Advances.Length);
        min.Should().NotBeNull();

        var seq = min!.Advances;
        var copy = results.Copy();
        copy.Spawner.Count.CountSeed = countSeed;
        var _ = AdvanceRemoval.RunForwards(copy, seq, seed);
        var expect = copy.Results.Where(z => seq.SequenceEqual(z.Advances));
        expect.FirstOrDefault(z => z.Entity.IsShiny).Should().NotBeNull();
    }
}
