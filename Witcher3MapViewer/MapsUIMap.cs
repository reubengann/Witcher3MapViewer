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
        private readonly string pathToCircle;
        Dictionary<string, int> IconPathToBitmapRegistryIdMap = new Dictionary<string, int>();
        Mapsui.Layers.MemoryLayer? CircleLayer;

        public MapsUIMap(MapControl control, string pathToCircle)
        {
            if (!File.Exists(pathToCircle))
                throw new FileNotFoundException("Could not find circle.png");
            this.control = control;
            this.pathToCircle = pathToCircle;
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
            AddCircleLayer(pathToCircle);
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

        public void CenterMap(double x, double y)
        {
            Mapsui.Geometries.Point center = new Mapsui.Geometries.Point(x, y);
            control.Navigator.NavigateTo(center, control.Map.Resolutions[4], 1000);
            AddCircleAt(center);
        }

        private void AddCircleLayer(string pathToCircle)
        {
            if (CircleLayer == null)
            {
                Mapsui.Layers.MemoryLayer _pointlayer = new Mapsui.Layers.MemoryLayer();
                List<Mapsui.Geometries.Point> _points = new List<Mapsui.Geometries.Point>();
                _pointlayer.DataSource = new Mapsui.Providers.MemoryProvider(_points);
                _pointlayer.Name = "Circle";
                SymbolStyle _style = new SymbolStyle();
                if (!IconPathToBitmapRegistryIdMap.ContainsKey(pathToCircle))
                    RegisterBitmap(pathToCircle);
                _style.BitmapId = IconPathToBitmapRegistryIdMap[pathToCircle];
                _pointlayer.Style = _style;
                CircleLayer = _pointlayer;
            }
            control.Map.Layers.Add(CircleLayer);
        }

        private void AddCircleAt(Mapsui.Geometries.Point Center)
        {
            if (CircleLayer == null) return;
            CircleLayer.DataSource = new Mapsui.Providers.MemoryProvider(Center);
            CircleLayer.Enabled = true;
        }
    }
}
