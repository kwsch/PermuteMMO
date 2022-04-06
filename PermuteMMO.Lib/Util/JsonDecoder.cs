using System.Globalization;
using Newtonsoft.Json;
using PKHeX.Core;

namespace PermuteMMO.Lib;

/// <summary>
/// Decodes the community's json data (ripped from pkNX) with some conversion back to primitives instead of strings.
/// </summary>
public static class JsonDecoder
{
    /// <summary>
    /// Wrapper to deserialize the json using whatever package this project is currently using.
    /// </summary>
    public static T Deserialize<T>(string json) where T : class => JsonConvert.DeserializeObject<T>(json);

    /// <summary>
    /// Converts the json string back to a usable dictionary.
    /// </summary>
    public static Dictionary<ulong, SlotDetail[]> GetDictionary(string json)
    {
        var obj = JsonConvert.DeserializeObject<Dictionary<string, SlotDetail[]>>(json);
        var result = new Dictionary<ulong, SlotDetail[]>(obj.Count);
        foreach (var (key, value) in obj)
        {
            var hash = ulong.Parse(key[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            foreach (var slot in value)
                slot.SetSpecies();
            result.Add(hash, value);
        }
        return result;
    }
}

/// <summary>
/// Encounter slot detail.
/// </summary>
/// <param name="Rate">Weight factor used to determine how frequent the encounter is yielded.</param>
/// <param name="Name">Community label name with Species-Form</param>
/// <param name="IsAlpha">Indicates if it is an alpha</param>
/// <param name="Level">Level range array</param>
/// <param name="FlawlessIVs">Amount of flawless IVs</param>
public sealed record SlotDetail(
    [property: JsonProperty("slot")] int Rate,
    [property: JsonProperty("name")] string Name,
    [property: JsonProperty("alpha")] bool IsAlpha,
    [property: JsonProperty("level")] IReadOnlyList<int> Level,
    [property: JsonProperty("ivs")] int FlawlessIVs
)
{
    public int LevelMin => Level[0];
    public int LevelMax => Level[1];
    public ushort Species { get; private set; }
    public ushort Form { get; private set; }
    public bool IsSkittish => BehaviorUtil.Skittish.Contains(Species);

    /// <summary>
    /// Parses the string name into actual indexes.
    /// </summary>
    public void SetSpecies()
    {
        try
        {
            string species;
            var dash = Name.IndexOf('-');
            if (dash > 0)
            {
                Form = ushort.Parse(Name.AsSpan(dash+1));
                species = Name[..dash];
            }
            else
            {
                species = Name;
            }

            if (species == "MimeJr.") // STOP FAKE NAMING SPECIES
                species = "Mime Jr.";
            if (species == "Mr.Mime") // STOP FAKE NAMING SPECIES
                species = "Mr. Mime";

            Species = (ushort)SpeciesName.SpeciesDict[(int)LanguageID.English][species];
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
