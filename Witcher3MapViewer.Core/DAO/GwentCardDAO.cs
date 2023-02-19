using System.Xml.Serialization;
using Witcher3MapViewer.Core.Readers;

namespace Witcher3MapViewer.Core.DAO
{
    [XmlRoot("gwentcards")]
    public class GwentCardSets
    {
        [XmlElement("set")]
        public List<GwentSet> Sets { get; set; }
    }

    public class GwentSet
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("card")]
        public List<GwentCardAsRead> Cards { get; set; }
    }

    internal class GwentCardDAO
    {
        [XmlAttribute("id")]
        public int ID { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("loc")]
        public string Location { get; set; }
    }
}
