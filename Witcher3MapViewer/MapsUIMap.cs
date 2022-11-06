using BruTile.MbTiles;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.UI.Wpf;
using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public void LoadMarkers(MarkerSpec markerSpec)
        {
            MemoryLayer pointlayer = new MemoryLayer { Style = null };
            List<Mapsui.Geometries.Point> _points = markerSpec.WorldLocations.Select(
                p => new Mapsui.Geometries.Point(p.X, p.Y)
                ).ToList();
            string path = markerSpec.ImagePath;
            if (!IconPathToBitmapRegistryIdMap.ContainsKey(path))
                RegisterBitmap(path);

            pointlayer.DataSource = new Mapsui.Providers.MemoryProvider(_points);
            SymbolStyle _style = new SymbolStyle
            {
                BitmapId = IconPathToBitmapRegistryIdMap[path]
            };
            pointlayer.Style = _style;
            pointlayer.Name = "foo";
            control.Map.Layers.Add(pointlayer);
        }

        public void SetLayerVisibility(int layerNumber, bool visible)
        {
            control.Map.Layers[layerNumber].Enabled = visible;
        }

        private void RegisterBitmap(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                MemoryStream ms = new MemoryStream();
                fs.CopyTo(ms);
                int id = BitmapRegistry.Instance.Register(ms);
                IconPathToBitmapRegistryIdMap[path] = id;
            }
        }
    }
}
