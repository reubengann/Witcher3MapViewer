using Prism.Commands;
using System.Windows.Input;

namespace Witcher3MapViewer.Core
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly IMap _map;
        private readonly IMarkerProvider _markerProvider;
        private readonly IMapSettingsProvider _mapSettingsProvider;
        private readonly IQuestListProvider _questListProvider;
        private readonly IQuestAvailabilityProvider _availabilityProvider;
        private readonly IGwentCardProvider gwentCardProvider;
        private readonly IGwentStatusProvider gwentStatusProvider;
        private readonly Dictionary<string, WorldSetting> TileMapPathMap;
        Dictionary<string, string> shortToLongNameMap;

        public MainWindowViewModel(IMap map,
            IMarkerProvider markerProvider,
            IMapSettingsProvider mapSettingsProvider,
            IQuestListProvider questListProvider,
            IQuestAvailabilityProvider availabilityProvider,
            IGwentCardProvider gwentCardProvider,
            ILevelProvider levelProvider,
            IGwentStatusProvider gwentStatusProvider
            )
        {
            _map = map;
            _markerProvider = markerProvider;
            _mapSettingsProvider = mapSettingsProvider;
            _questListProvider = questListProvider;
            _availabilityProvider = availabilityProvider;
            this.gwentCardProvider = gwentCardProvider;
            this.gwentStatusProvider = gwentStatusProvider;
            List<WorldSetting> worldSettings = _mapSettingsProvider.GetAll();
            shortToLongNameMap = worldSettings.ToDictionary(x => x.ShortName, x => x.Name);
            shortToLongNameMap["VE"] = shortToLongNameMap["NO"];
            ListOfMaps = worldSettings.Select(x => x.Name).ToList();
            TileMapPathMap = worldSettings.ToDictionary(x => x.Name, x => x);
            MarkerToggleViewModel = new MarkerToggleViewModel(_map);

            Func<Quest, bool> showAllAvailablePolicy = q => _availabilityProvider.IsQuestAvailable(q);
            Func<Quest, bool> showAvailableAndUndonePolicy = q => _availabilityProvider.IsQuestAvailable(q) && _availabilityProvider.GetState(q.GUID) < QuestStatusState.Success;
            QuestListViewModel = new QuestListViewModel(questListProvider.GetAllQuests(), availabilityProvider, levelProvider, showAvailableAndUndonePolicy);
            QuestListViewModel.ItemSelectedChanged += QuestListViewModel_ItemSelectedChanged;
            QuestListViewModel.SelectBest();
        }

        private void QuestListViewModel_ItemSelectedChanged(QuestViewModel obj)
        {
            var longName = shortToLongNameMap[obj._quest.World];
            if (SelectedMap != longName)
            {
                SelectedMap = longName;
            }
            var quest = obj._quest;
            if (quest.DiscoverPrompt == null)
            {
                InfoMessage = "Advance main quest";
                return;
            }
            var p = quest.DiscoverPrompt;
            if (p.Location != null)
            {
                var worldpoint = RealToGameSpaceConversion.ToWorldSpace(new Point(p.Location.X, p.Location.Y), TileMapPathMap[SelectedMap]);
                _map.CenterMap(worldpoint.X, worldpoint.Y);
            }
            if (quest.DiscoverPrompt != null)
                InfoMessage = quest.DiscoverPrompt.Info;
            else InfoMessage = string.Empty;
        }

        private string _infoMessage;

        public string InfoMessage
        {
            get { return _infoMessage; }
            set { _infoMessage = value; OnPropertyChanged(nameof(InfoMessage)); }
        }

        public string GwentMessage
        {
            get
            {
                int totalCount = 0;
                int totalOwned = 0;
                foreach (var g in gwentCardProvider.GetGwentCards())
                {
                    totalCount++;
                    if (gwentStatusProvider.GetCount(g.cardIndex) > 0) totalOwned++;
                }
                return $"Gwent cards {totalOwned}/{totalCount}";
            }
        }

        public ICommand LoadInitialMapCommand { get => new DelegateCommand(LoadInitialMap); }
        public List<string> ListOfMaps { get; set; }

        public MarkerToggleViewModel MarkerToggleViewModel { get; set; }
        public QuestListViewModel QuestListViewModel { get; set; }

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
            MarkerToggleViewModel.Clear();
            WorldSetting worldSetting = TileMapPathMap[SelectedMap];
            _map.LoadMap(worldSetting.TileSource);
            MarkerSpec? roadsign = null;
            List<MarkerSpec> layers = _markerProvider.GetMarkerSpecs(worldSetting.ShortName);
            int layerNumber = 1;
            foreach (var layer in layers)
            {
                if (layer.type == "RoadSign")
                    roadsign = layer;
                else
                {
                    _map.LoadMarkers(layer);
                    MarkerToggleViewModel.AddItemToRoot(layer.FullName, layer.ImagePath, layerNumber);
                    layerNumber++;
                }
            }
            if (roadsign != null) _map.LoadMarkers(roadsign);
        }

        private void LoadInitialMap()
        {
            if (string.IsNullOrEmpty(SelectedMap))
                SelectedMap = ListOfMaps.First();
        }
    }
}
