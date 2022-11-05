using BruTile.MbTiles;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.UI.Wpf;
using SQLite;
using System.Collections.Generic;
using System.IO;
using Witcher3MapViewer.Core;

namespace Witcher3MapViewer.WPF
{
    public class MapsUIMap : IMap
    {
        private readonly MapControl control;
        Dictionary<string, int> IconPathToBitmapRegistryIdMap = new Dictionary<string, int>();

        public MapsUIMap(MapControl control)
        {
            this.control = control;
        }

        public void LoadMap(string path)
        {
            MbTilesTileSource _source = new MbTilesTileSource(new SQLiteConnectionString(path, false));
            TileLayer tileLayer = new TileLayer(_source);
            control.Map.Layers.Clear();
            control.Map.Layers.Add(tileLayer);
        }

        public void LoadMarkers()
        {
            MemoryLayer pointlayer = new MemoryLayer { Style = null };
            List<Mapsui.Geometries.Point> _points = new List<Mapsui.Geometries.Point> { new Mapsui.Geometries.Point(0, 0) };
            string path = @"C:\repos\Witcher3MapViewer\Witcher3MapViewer\MarkerImages\RoadSign.png";
            if (!IconPathToBitmapRegistryIdMap.ContainsKey(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    MemoryStream ms = new MemoryStream();
                    fs.CopyTo(ms);
                    int id = BitmapRegistry.Instance.Register(ms);
                    IconPathToBitmapRegistryIdMap[path] = id;
                }
            }
            pointlayer.DataSource = new Mapsui.Providers.MemoryProvider(_points);
            SymbolStyle _style = new SymbolStyle
            {
                BitmapId = IconPathToBitmapRegistryIdMap[path]
            };
            pointlayer.Style = _style;
            pointlayer.Name = "foo";
            control.Map.Layers.Add(pointlayer);
        }
    }
}
