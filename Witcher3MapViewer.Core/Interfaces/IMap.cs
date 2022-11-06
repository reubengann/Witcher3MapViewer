namespace Witcher3MapViewer.Core
{
    public class MarkerSpec
    {
        public string ImagePath;
        public List<Point> WorldLocations;
        public string type;

        public MarkerSpec(string imagePath, List<Point> worldLocations, string type)
        {
            ImagePath = imagePath;
            WorldLocations = worldLocations;
            this.type = type;
        }
    }

    public interface IMap
    {
        void LoadMap(string path);
        void LoadMarkers(MarkerSpec markerSpec);
    }
}
