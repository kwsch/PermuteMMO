using FluentAssertions;
using PermuteMMO.Reversal;
using PKHeX.Core;
using Xunit;

namespace PermuteMMO.Tests;

public class ReversalTests
{
    [Fact]
    public void TestReversal()
    {
        var pk1 = Properties.Resources.Tentacool1;
        var pk2 = Properties.Resources.Tentacool2;
        var pk3 = Properties.Resources.Tentacool3;
        var pk4 = Properties.Resources.Tentacool4;

        var all = new[] { pk1, pk2, pk3, pk4 };
        foreach (var d in all)
        {
            var pa8 = new PA8(d);
            var seeds = IterativeReversal.GetSeeds(pa8, 32);
            seeds.Should().NotBeEmpty();
        }
    }
}
