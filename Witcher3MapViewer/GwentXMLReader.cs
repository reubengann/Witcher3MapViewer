using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Witcher3MapViewer.Readers
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

    public class GwentCardAsRead
    {
        [XmlAttribute("id")]
        public int ID { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("loc")]
        public string Location { get; set; }

        [XmlElement("quest")]
        public string AssociatedQuest { get; set; }
    }


    public class GwentXMLReader
    {
        public List<GwentSet> Sets;

        public GwentXMLReader(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GwentCardSets));
            using (Stream reader = new FileStream(filename, FileMode.Open))
            {
                GwentCardSets thesets = (GwentCardSets)serializer.Deserialize(reader);
                Sets = thesets.Sets;
            }
        }        

    }
}
