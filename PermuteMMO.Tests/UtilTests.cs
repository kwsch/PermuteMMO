using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using PermuteMMO.Lib;
using PKHeX.Core;
using Xunit;
using static PermuteMMO.Lib.Advance;

namespace PermuteMMO.Tests;

public static class UtilTests
{
    [Fact]
    public static void CreateJson()
    {
        var obj = new UserEnteredSpawnInfo
        {
            Species = (int)Species.Diglett,
            Seed = 0xDEADBABE_BEEFCAFE.ToString(),
            BaseCount = 10,
            BaseTable = $"0x{0x1122_10F4_7DE9_8115:X16}",
            BonusCount = 0,
            BonusTable = $"0x{0:X16}",
        };

        var fileName = Path.Combine(Environment.CurrentDirectory, "spawner.json");
        var settings = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Populate };
        var result = JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
        File.WriteAllText(fileName, result);

        string argument = "/select, \"" + fileName + "\"";
        Process.Start("explorer.exe", argument);
    }

    [Fact]
    public static void Garchomp()
    {
        var spawn = SpawnInfo.GetMMO(0x85714105CF348588, 9, 0x8AE0881E5F939184, 7);
        const ulong seed = 12880307074085126207u;
        var sequence = new[] { A2, A1, A3 };
        const int index = 2;

        var gs = Calculations.GetGroupSeed(seed, sequence);
        var respawnSeed = Calculations.GetGenerateSeed(gs, index);
        var entitySeed = Calculations.GetEntitySeed(gs, index);
        if (!spawn.GetNextWave(out var next))
            throw new Exception();
        var result = SpawnGenerator.Generate(respawnSeed, next.Set.Table, next.Detail.SpawnType);
        result.IsShiny.Should().BeTrue();
        result.IsAlpha.Should().BeTrue();
        entitySeed.Should().Be(0xc50932b428a734fd);

        var permute = Permuter.Permute(spawn, seed);
        var match = permute.Results.Find(z => z.Entity.Seed == respawnSeed);
        match.Should().NotBeNull();
    }

    [Fact]
    public static void Stantler()
    {
        var spawn = SpawnInfo.GetMMO(0x5BFA9CCA4ED8142B, 10, 0xC213942F6D31614C, 6);
        const ulong seed = 88514016295302425u;

        // Spawn 4 pokemon
        var entities = new List<EntityResult>();
        for (int i = 1; i <= 4; i++)
        {
            var genSeed = Calculations.GetGenerateSeed(seed, i);
            var entity = SpawnGenerator.Generate(genSeed, spawn.Set.Table, spawn.Detail.SpawnType);
            entities.Add(entity);
        }

        var count = entities.Count(z => z.IsAggressive);
        count.Should().Be(2);
    }
}
