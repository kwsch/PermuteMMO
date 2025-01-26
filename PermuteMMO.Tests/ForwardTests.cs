using FluentAssertions;
using PermuteMMO.Lib;
using Xunit;
using static PermuteMMO.Lib.Advance;

namespace PermuteMMO.Tests;

/// <summary>
/// Aren't these nice to have? Examples??
/// </summary>
public sealed class ForwardTests
{
    [Theory]
    [InlineData(0xA5D779D8831721FD, 10, 6)]
    public void First(in ulong seed, in int baseCount, in int bonusCount)
    {
        var spawner = SpawnInfo.GetMMO(0x7FA3A1DE69BD271E, baseCount, 0x44182B854CD3745D, bonusCount);
        var result = Permuter.Permute(spawner, seed);
        result.Results.Find(z => z.Entity.PID == 0x6f4edff0).Should().NotBeNull();

        var first = result.Results[0];
        var match = RunForwardsRegenerate(seed, result, first);
        Assert.NotNull(match);
        match.Entity.SlotSeed.Should().Be(first.Entity.SlotSeed);
    }

    private static PermuteResult? RunForwardsRegenerate(ulong seed, PermuteMeta result, PermuteResult first)
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
        Advance[] seq = [A1, A1, A2, A4, CR, A2, A2];

        _ = AdvanceRemoval.RunForwards(result, seq, seed);
        var expect = result.Results.Where(z => seq.SequenceEqual(z.Advances));
        expect.FirstOrDefault(z => z.Entity is { IsShiny: true, Index: 2 }).Should().NotBeNull();
    }

    [Theory]
    [InlineData(7345882244663053439u, 9, 7)]
    public void TestForwardsBeta(in ulong seed, in int baseCount, in int bonusCount)
    {
        var spawner = SpawnInfo.GetMMO(0x03B40EB53F427739, baseCount, 0xDF3114CC95FDCF22, bonusCount);
        var result = Permuter.Permute(spawner, seed).Copy();
        Advance[] seq = [B1, B1, B1, B1, B1, G1, CR, A1, A4];

        _ = AdvanceRemoval.RunForwards(result, seq, seed);
        var expect = result.Results.Where(z => seq.SequenceEqual(z.Advances));
        expect.FirstOrDefault(z => z.Entity is { IsShiny: true, Index: 4 }).Should().NotBeNull();

        var copy = Permuter.Permute(spawner, seed).Copy();
        copy.Criteria = (_, _) => true;
        var __ = AdvanceRemoval.RunForwards(copy, seq, seed);
        var lines = PermuteDump.Dump(copy);
        var msg = string.Join(Environment.NewLine, lines);
    }
}
