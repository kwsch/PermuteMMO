using FluentAssertions;
using PermuteMMO.Lib;
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
        var spawner = new SpawnInfo
        {
            BaseCount = baseCount,
            BaseTable = 0x7FA3A1DE69BD271E,

            BonusCount = bonusCount,
            BonusTable = 0x44182B854CD3745D,
        };

        var result = Permuter.Permute(spawner, seed);
        result.Results.Find(z => z.Entity.PID == 0x6f4edff0).Should().NotBeNull();

        ConsolePermuter.PermuteSingle(spawner, seed);
    }
}
