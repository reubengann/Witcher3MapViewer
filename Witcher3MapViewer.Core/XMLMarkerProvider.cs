using System.Xml.Serialization;

namespace Witcher3MapViewer.Core
{
    public class XMLMarkerProvider : IMarkerProvider
    {
        private Dictionary<string, List<MarkerSpec>> Items;
        IconSettings iconSettings;
        Dictionary<string, string> filenameLookup;
        private readonly IMapSettingsProvider mapSettingsProvider;

        public XMLMarkerProvider(Stream s, IMapSettingsProvider mapSettingsProvider)
        {
            this.mapSettingsProvider = mapSettingsProvider;
            Items = new Dictionary<string, List<MarkerSpec>>();
            XmlSerializer serializer = new XmlSerializer(typeof(MapPinCollectionDAO));
            MapPinCollectionDAO? readitems = (MapPinCollectionDAO?)serializer.Deserialize(s);
            if (readitems == null) throw new Exception();
            iconSettings = mapSettingsProvider.GetIconSettings();
            filenameLookup = new Dictionary<string, string>();
            foreach (IconInfo? iconInfo in iconSettings.IconInfos)
            {
                filenameLookup[iconInfo.InternalName] = iconInfo.Image;
                if (iconInfo.Aliases != null)
                {
                    foreach (string alias in iconInfo.Aliases)
                        filenameLookup[alias] = iconInfo.Image;

                }
            }
            foreach (MapPinWorldDAO worldDAO in readitems.Worlds)
            {
                Items[worldDAO.Code] = GroupMarkerTypes(worldDAO, worldDAO.Code);
            }

        }

        private List<MarkerSpec> GroupMarkerTypes(MapPinWorldDAO worldDAO, string worldShortName)
        {
            ILookup<string, MapPinDAO>? groups = worldDAO.Pins.ToLookup(x => x.Type);
            return groups.Select(x => new MarkerSpec(FindPathToIcon(x.Key), x.Select(p => ComputeMapPosition(p.Position, worldShortName)).ToList())).ToList();
        }

        private string FindPathToIcon(string key)
        {
            string iconFilename = filenameLookup[key];
            return Path.Combine(iconSettings.LargeIconPath, iconFilename);
        }

        private Point ComputeMapPosition(MapPinPositionDAO worldPosition, string worldShortName)
        {
            WorldSetting worldSetting = mapSettingsProvider.GetWorldSetting(worldShortName);
            return new Point(
                worldPosition.x * worldSetting.Slope + worldSetting.XIntercept,
                worldPosition.y * worldSetting.Slope + worldSetting.YIntercept
                );
        }

        public List<MarkerSpec> GetMarkerSpecs(string worldName)
        {
            return Items[worldName];
        }

        public static XMLMarkerProvider FromFile(string filepath, IMapSettingsProvider mapSettingsProvider)
        {
            using FileStream sr = new FileStream(filepath, FileMode.Open);
            return new XMLMarkerProvider(sr, mapSettingsProvider);
        }
    }

    [XmlRoot("mappins")]
    public class MapPinCollectionDAO
    {
        [XmlElement("world")]
        public List<MapPinWorldDAO> Worlds { get; set; }
    }

    public class MapPinWorldDAO
    {
        [XmlAttribute("code")]
        public string Code { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("mappin")]
        public List<MapPinDAO> Pins { get; set; }
    }

    public class MapPinDAO
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlElement("position")]
        public MapPinPositionDAO Position { get; set; }

        [XmlElement("internalname")]
        public string InternalName { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

    }

    public class MapPinPositionDAO
    {
        [XmlAttribute("x")]
        public int x { get; set; }

        [XmlAttribute("y")]
        public int y { get; set; }
    }

}
