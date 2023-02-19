namespace Witcher3MapViewer.Core
{
    public interface IGwentCardProvider
    {
        List<GwentCard> GetGwentCards();
    }

    public class GwentCard
    {
        public int cardIndex;
        public string Name;
        public string Location;

        public GwentCard(int cardIndex, string name, string location)
        {
            this.cardIndex = cardIndex;
            Name = name;
            Location = location;
        }
    }
}
