using Prism.Commands;
using System.Windows.Input;

namespace Witcher3MapViewer.Core
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly IMap _map;
        private readonly IMarkerProvider _markerProvider;

        public MainWindowViewModel(IMap map, IMarkerProvider markerProvider)
        {
            _map = map;
            _markerProvider = markerProvider;
        }

        public ICommand LoadInitialMapCommand { get => new DelegateCommand(LoadInitialMap); }
        public List<string> ListOfMaps { get; set; } = new List<string>(MapInfo.ListOfMaps);



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
            _map.LoadMap(MapInfo.TileMapPathMap[SelectedMap]);
            _map.LoadMarkers(MapMarkers.MapMarkerSpec);
        }

        private void LoadInitialMap()
        {
            SelectedMap = ListOfMaps.First();
        }
    }
}
