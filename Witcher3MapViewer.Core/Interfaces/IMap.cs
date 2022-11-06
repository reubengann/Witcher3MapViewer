namespace Witcher3MapViewer.Core
{
    public class MarkerSpec
    {
        public string ImagePath;
        public List<Point> WorldLocations;
        public string type;
        public string FullName;

        public MarkerSpec(string imagePath, List<Point> worldLocations, string type, string fullName)
        {
            ImagePath = imagePath;
            WorldLocations = worldLocations;
            this.type = type;
            FullName = fullName;
        }
    }

    public interface IMap
    {
        void LoadMap(string path);
        void LoadMarkers(MarkerSpec markerSpec);
    }
}
