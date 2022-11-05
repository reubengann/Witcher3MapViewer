namespace Witcher3MapViewer.Core
{
    public static class MapInfo
    {
        public static Dictionary<string, string> TileMapPathMap = new Dictionary<string, string>
        {
            { "White Orchard" , @"C:\repos\Witcher3MapViewer\Witcher3MapViewer\Maps\WhiteOrchard.mbtiles" },
            { "Velen/Novigrad" , @"C:\repos\Witcher3MapViewer\Witcher3MapViewer\Maps\VelenNovigrad.mbtiles" },
            { "Skellige" , @"C:\repos\Witcher3MapViewer\Witcher3MapViewer\Maps\Skellige.mbtiles" },
            { "Kaer Morhen" , @"C:\repos\Witcher3MapViewer\Witcher3MapViewer\Maps\KaerMorhen.mbtiles" },
            { "Toussaint" , @"C:\repos\Witcher3MapViewer\Witcher3MapViewer\Maps\Toussaint.mbtiles" },
        };

        public static List<string> ListOfMaps { get; set; } = new List<string>
        {
            "White Orchard",
            "Velen/Novigrad",
            "Skellige",
            "Kaer Morhen",
            "Toussaint"
        };
    }
}
