namespace Witcher3MapViewer.Core
{
    public interface IMap
    {
        void LoadMap(string path);
        void LoadMarkers();
    }
}
