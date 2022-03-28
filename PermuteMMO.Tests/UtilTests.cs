﻿using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using PermuteMMO.Lib;
using PKHeX.Core;
using Xunit;

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
}
