namespace Witcher3MapViewer.Core
{
    public class MarkerSpec
    {
        public string ImagePath;
        public List<Point> WorldLocations;

        public MarkerSpec(string imagePath, List<Point> worldLocations)
        {
            ImagePath = imagePath;
            WorldLocations = worldLocations;
        }
    }

    public interface IMap
    {
        void LoadMap(string path);
        void LoadMarkers(MarkerSpec markerSpec);
    }
}
