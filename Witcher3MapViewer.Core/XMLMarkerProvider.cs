using System.Xml.Serialization;

namespace Witcher3MapViewer.Core
{
    public class XMLMarkerProvider : IMarkerProvider
    {
        private Dictionary<string, List<MarkerSpec>> Items;

        public XMLMarkerProvider(Stream s)
        {
            Items = new Dictionary<string, List<MarkerSpec>>();
            XmlSerializer serializer = new XmlSerializer(typeof(MapPinCollectionDAO));
            MapPinCollectionDAO? readitems = (MapPinCollectionDAO?)serializer.Deserialize(s);
            if (readitems == null) throw new Exception();
            foreach (MapPinWorldDAO worldDAO in readitems.Worlds)
            {
                Items[worldDAO.Code] = new List<MarkerSpec>();
            }
        }

        public List<MarkerSpec> GetMarkerSpecs(string worldCode)
        {
            return Items[worldCode];
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
