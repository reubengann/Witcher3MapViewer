using System.Xml.Serialization;

namespace Witcher3MapViewer.Core.Readers
{
    class MapPinReader
    {
        List<MapPinWorld> Worlds;
        Dictionary<MapPinWorld, Dictionary<MapPinType, MapPinCollection>> WorldData;

        public MapPinReader(string filename, Dictionary<string, MapPinType> TypeLookup)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MapPinCollectionAsRead));
            Worlds = new List<MapPinWorld>();
            using (Stream reader = new FileStream(filename, FileMode.Open))
            {
                MapPinCollectionAsRead readitems = (MapPinCollectionAsRead)serializer.Deserialize(reader);
                Worlds = readitems.Worlds;
            }
            WorldData = new Dictionary<MapPinWorld, Dictionary<MapPinType, MapPinCollection>>();
            foreach (MapPinWorld w in Worlds)
            {
                Dictionary<MapPinType, MapPinCollection> Collated = new Dictionary<MapPinType, MapPinCollection>();
                foreach (MapPin pin in w.Pins)
                {
                    if (!Collated.ContainsKey(TypeLookup[pin.Type]))
                        Collated[TypeLookup[pin.Type]] = new MapPinCollection();

                    Collated[TypeLookup[pin.Type]].Add(pin);
                }
                WorldData[w] = Collated;
            }
        }

        public Dictionary<MapPinType, MapPinCollection> GetPins(string code)
        {
            foreach (MapPinWorld w in WorldData.Keys)
            {
                if (w.Code == code)
                {
                    return WorldData[w];
                }
            }
            return null;
        }
    }

    [XmlRoot("mappins")]
    public class MapPinCollectionAsRead
    {
        [XmlElement("world")]
        public List<MapPinWorld> Worlds { get; set; }
    }

    public class MapPinWorld
    {
        [XmlAttribute("code")]
        public string Code { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("mappin")]
        public List<MapPin> Pins { get; set; }
    }

    public class MapPin
    {
        [XmlIgnore]
        public Point Location { get { return new Point(PositionAsRead.x, PositionAsRead.y); } }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlElement("position")]
        public MapPinPosition PositionAsRead { get; set; }

        [XmlElement("internalname")]
        public string InternalName { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

    }

    public class MapPinPosition
    {
        [XmlAttribute("x")]
        public int x { get; set; }

        [XmlAttribute("y")]
        public int y { get; set; }
    }

    class MapPinCollection
    {
        public List<MapPin> Locations { get; }

        public MapPinCollection()
        {
            Locations = new List<MapPin>();
        }

        public MapPinCollection(IEnumerable<MapPin> Pins)
        {
            Locations = new List<MapPin>(Pins);
        }

        public void Add(MapPin mappin)
        {
            Locations.Add(mappin);
        }

        public void AddRange(IEnumerable<MapPin> Collection)
        {
            Locations.AddRange(Collection);
        }
    }

    public class MapPinType
    {
        public string Name;
        public string InternalName;
        public string IconFile;
        public List<string> Aliases;
        public bool Shown = true;

        public MapPinType(string name, string internalname)
        {
            Name = name;
            InternalName = internalname;
            Aliases = new List<string>();
        }

        public MapPinType(string name, string internalname, List<string> aliases)
        {
            Name = name;
            InternalName = internalname;
            Aliases = aliases;
        }
    }
}
