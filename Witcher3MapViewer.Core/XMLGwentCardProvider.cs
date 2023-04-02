using System.Xml.Serialization;
using Witcher3MapViewer.Core.DAO;

namespace Witcher3MapViewer.Core
{
    public class XMLGwentCardProvider : IGwentCardProvider
    {
        List<GwentCard> _gwentCards;

        public XMLGwentCardProvider(Stream s)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GwentCardSets));
            GwentCardSets? readitems = (GwentCardSets?)serializer.Deserialize(s);
            if (readitems == null) throw new Exception();
            _gwentCards = new List<GwentCard>();
            foreach (var item in readitems.Sets[0].Cards)
            {
                _gwentCards.Add(new GwentCard(item.ID, item.Name, item.Location));
            }
        }

        public List<GwentCard> GetGwentCards()
        {
            return _gwentCards;
        }

        public static XMLGwentCardProvider FromFile(string filepath)
        {
            using FileStream sr = new FileStream(filepath, FileMode.Open);
            return new XMLGwentCardProvider(sr);
        }
    }
}
