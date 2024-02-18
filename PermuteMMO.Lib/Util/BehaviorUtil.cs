using static PKHeX.Core.Species;

namespace PermuteMMO.Lib;

public static class BehaviorUtil
{
    public static ReadOnlySpan<ushort> Oblivious =>
    [
        (ushort)Cyndaquil, // Cyndaquil and Hippopotas can be scared, but the game seems to only
        (ushort)Hippopotas, // ever spawn 1 that will run away and 3 that attack you instead.
        (ushort)Lickilicky,
        (ushort)Lickitung,
        (ushort)Magikarp,
        (ushort)MrMime,
    ];

    public static ReadOnlySpan<ushort> Skittish =>
    [
        (ushort)Abra,
        (ushort)Aipom,
        (ushort)Basculin,
        (ushort)Bidoof,
        (ushort)Blissey,
        (ushort)Bonsly,
        (ushort)Budew,
        (ushort)Buneary,
        (ushort)Chansey,
        (ushort)Chatot,
        (ushort)Cherubi,
        (ushort)Chimchar,
        (ushort)Chimecho,
        (ushort)Chingling,
        (ushort)Clefairy,
        (ushort)Cleffa,
        (ushort)Combee,
        (ushort)Eevee,
        (ushort)Finneon,
        (ushort)Froslass,
        (ushort)Gardevoir,
        (ushort)Glameow,
        (ushort)Goomy,
        (ushort)Happiny,
        (ushort)Kirlia,
        (ushort)Kricketot,
        (ushort)Kricketune,
        (ushort)Lickilicky,
        (ushort)Lickitung,
        (ushort)Lopunny,
        (ushort)Lumineon,
        (ushort)Magby,
        (ushort)Magikarp,
        (ushort)Mantine,
        (ushort)Mantyke,
        (ushort)MimeJr,
        (ushort)Misdreavus,
        (ushort)Mismagius,
        (ushort)MrMime,
        (ushort)Munchlax,
        (ushort)Pachirisu,
        (ushort)Petilil,
        (ushort)Pichu,
        (ushort)Piplup,
        (ushort)Ponyta,
        (ushort)Purugly,
        (ushort)Ralts,
        (ushort)Rowlet,
        (ushort)Shellos,
        (ushort)Sliggoo,
        (ushort)Snorunt,
        (ushort)Spheal,
        (ushort)Stantler,
        (ushort)Starly,
        (ushort)Sudowoodo,
        (ushort)Swinub,
        (ushort)Teddiursa,
        (ushort)Togepi,
        (ushort)Togetic,
        (ushort)Turtwig,
        (ushort)Unown,
        (ushort)Vulpix,
        (ushort)Wurmple,
    ];
}
