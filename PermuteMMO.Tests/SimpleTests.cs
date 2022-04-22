using System.Linq;
using FluentAssertions;
using PermuteMMO.Lib;
using PKHeX.Core;
using Xunit;

namespace PermuteMMO.Tests;

/// <summary>
/// Aren't these nice to have? Examples??
/// </summary>
public sealed class SimpleTests
{
    [Theory]
    [InlineData(0xA5D779D8831721FD, 10, 6)]
    public void First(in ulong seed, in int baseCount, in int bonusCount)
    {
        var spawner = SpawnInfo.GetMMO(0x7FA3A1DE69BD271E, baseCount, 0x44182B854CD3745D, bonusCount);
        var result = Permuter.Permute(spawner, seed);
        result.Results.Find(z => z.Entity.PID == 0x6f4edff0).Should().NotBeNull();

        var first = result.Results[0];
        var match = RunFowardsRegenerate(seed, result, first);
        Assert.NotNull(match);
        match!.Entity.SlotSeed.Should().Be(first.Entity.SlotSeed);
    }

    private static PermuteResult? RunFowardsRegenerate(ulong seed, PermuteMeta result, PermuteResult first)
    {
        result = result.Copy(); // disassociate but keep same inputs
        result.Criteria = (_, _) => true;
        var (advances, entityResult) = first;
        var steps = AdvanceRemoval.RunForwards(result, advances, seed);
        steps.Count.Should().BeGreaterThan(0);

        return result.Results.Find(z => advances.SequenceEqual(z.Advances) && entityResult.Index == z.Entity.Index);
    }

    [Theory]
    [InlineData(1911689355633755303u, 9, 7)]
    public void TestForwards(in ulong seed, in int baseCount, in int bonusCount)
    {
        var spawner = SpawnInfo.GetMMO(0xECBF77B8F7302126, baseCount, 0x9D713CCF138FD43C, bonusCount);
        var result = Permuter.Permute(spawner, seed).Copy();
        var seq = new[] { Advance.A1, Advance.A1, Advance.A2, Advance.A4, Advance.CR, Advance.A2, Advance.A2 };

        var _ = AdvanceRemoval.RunForwards(result, seq, seed);
        var expect = result.Results.Where(z => seq.SequenceEqual(z.Advances));
        expect.FirstOrDefault(z => z.Entity.IsShiny && z.Entity.Index == 2).Should().NotBeNull();
    }

    [Theory]
    [InlineData(0xB2204D9BA549D169u)]
    public void TestMulti(in ulong seed)
    {
        var combee = new SlotDetail[]
        {
            new(100, "Combee", false, new [] {17, 20}, 0),
            new(2, "Combee", true , new [] {32, 35}, 3),
        };
        foreach (var s in combee)
            s.SetSpecies();

        const ulong key = 0x1337BABECAFEDEAD;
        SpawnGenerator.EncounterTables.Add(key, combee);

        const int count = 2;
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
    [InlineData(0xA0B404668A34A9E6u, 0x6B9EBA2AD7437ADEu)]
    public void TestUnown(in ulong seed, in ulong countSeed)
    {
        SlotDetail[] slots = new SlotDetail[28 * 2];
        const ulong key = 9489890319879407414;
        for (int i = 0; i < slots.Length/2; i++)
        {
            var name = $"Unown{(i == 0 ? "" : $"-{i}")}";
            slots[i]      = new(100, name, false, new[] { 25, 25 }, 0);
            slots[i + 28] = new(001, name, true , new[] { 25, 25 }, 3);
        }
        SetFakeTable(slots, key);

        const int minCount = 2;
        const int maxCount = 3;
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

    private static void SetFakeTable(SlotDetail[] slots, ulong key)
    {
        foreach (var s in slots)
            s.SetSpecies();

        SpawnGenerator.EncounterTables.Add(key, slots);
    }

    [Theory]
    [InlineData(0x9C1107A569F7681D)]
    public void TestEevee(in ulong seed)
    {
        var combee = new SlotDetail[]
        {
            new(100, "Bidoof", false, new [] {3, 6}, 0),
            new(2, "Bidoof", true , new [] {17, 19}, 3),
            new(20, "Eevee", false, new [] {3, 6}, 0),
            new(1, "Eevee", true , new [] {17, 19}, 3),
        };
        foreach (var s in combee)
            s.SetSpecies();

        const ulong key = 0x1337BABE12345678;
        SpawnGenerator.EncounterTables.Add(key, combee);

        const int rolls = 5;
        const int count = 2;
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
}