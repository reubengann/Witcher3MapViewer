using BruTile.MbTiles;
using Mapsui.Layers;
using Mapsui.UI.Wpf;
using SQLite;
using Witcher3MapViewer.Core;

namespace Witcher3MapViewer.WPF
{
    public class MapsUIMap : IMap
    {
        private readonly MapControl control;

        public MapsUIMap(MapControl control)
        {
            this.control = control;
        }

        public void LoadMap(string path)
        {
            MbTilesTileSource _source = new MbTilesTileSource(new SQLiteConnectionString(path, false));
            TileLayer tileLayer = new TileLayer(_source);
            control.Map.Layers.Add(tileLayer);
        }
    }
}
