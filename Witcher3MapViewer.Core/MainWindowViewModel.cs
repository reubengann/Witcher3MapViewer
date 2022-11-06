using Prism.Commands;
using System.Windows.Input;

namespace Witcher3MapViewer.Core
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly IMap _map;
        private readonly IMarkerProvider _markerProvider;
        private readonly IMapSettingsProvider _mapSettingsProvider;
        private readonly Dictionary<string, WorldSetting> TileMapPathMap;

        public MainWindowViewModel(IMap map, IMarkerProvider markerProvider, IMapSettingsProvider mapSettingsProvider)
        {
            _map = map;
            _markerProvider = markerProvider;
            _mapSettingsProvider = mapSettingsProvider;
            List<WorldSetting> worldSettings = _mapSettingsProvider.GetAll();
            ListOfMaps = worldSettings.Select(x => x.Name).ToList();
            TileMapPathMap = worldSettings.ToDictionary(x => x.Name, x => x);
        }

        public ICommand LoadInitialMapCommand { get => new DelegateCommand(LoadInitialMap); }
        public List<string> ListOfMaps { get; set; }

        public MarkerToggleViewModel MarkerToggleViewModel { get; set; } = new MarkerToggleViewModel();

        private string _selectedMap = "";

        public string SelectedMap
        {
            get { return _selectedMap; }
            set
            {
                _selectedMap = value;
                OnPropertyChanged(nameof(SelectedMap));
                UpdateMap();
            }
        }

        private void UpdateMap()
        {
            WorldSetting worldSetting = TileMapPathMap[SelectedMap];
            _map.LoadMap(worldSetting.TileSource);
            MarkerSpec? roadsign = null;
            foreach (var layer in _markerProvider.GetMarkerSpecs(worldSetting.ShortName))
            {
                if (layer.type == "RoadSign")
                    roadsign = layer;
                else _map.LoadMarkers(layer);
            }
            if (roadsign != null) _map.LoadMarkers(roadsign);
        }

        private void LoadInitialMap()
        {
            SelectedMap = ListOfMaps.First();
        }
    }
}
