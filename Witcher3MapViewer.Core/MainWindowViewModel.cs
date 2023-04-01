using Prism.Commands;
using System.Windows.Input;
using Witcher3MapViewer.Core.Interfaces;

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
        private readonly ILevelProvider levelProvider;
        private readonly IGwentStatusProvider gwentStatusProvider;
        private readonly IGwentTrackerWindow gwentTrackerWindow;
        private readonly IOptionsDialogWindow optionsDialogWindow;
        private readonly OptionsStore policyStore;
        private readonly Dictionary<string, WorldSetting> TileMapPathMap;
        Dictionary<string, string> shortToLongNameMap;

        public MainWindowViewModel(IMap map,
            IMarkerProvider markerProvider,
            IMapSettingsProvider mapSettingsProvider,
            IQuestListProvider questListProvider,
            IQuestAvailabilityProvider availabilityProvider,
            IGwentCardProvider gwentCardProvider,
            ILevelProvider levelProvider,
            IGwentStatusProvider gwentStatusProvider,
            IGwentTrackerWindow gwentTrackerWindow,
            IOptionsDialogWindow optionsDialogWindow,
            OptionsStore policyStore
            )
        {
            _map = map;
            _markerProvider = markerProvider;
            _mapSettingsProvider = mapSettingsProvider;
            _questListProvider = questListProvider;
            _availabilityProvider = availabilityProvider;
            this.gwentCardProvider = gwentCardProvider;
            this.levelProvider = levelProvider;
            this.gwentStatusProvider = gwentStatusProvider;
            this.gwentTrackerWindow = gwentTrackerWindow;
            this.optionsDialogWindow = optionsDialogWindow;
            this.policyStore = policyStore;
            List<WorldSetting> worldSettings = _mapSettingsProvider.GetAll();
            shortToLongNameMap = worldSettings.ToDictionary(x => x.ShortName, x => x.Name);
            shortToLongNameMap["VE"] = shortToLongNameMap["NO"];
            ListOfMaps = worldSettings.Select(x => x.Name).ToList();
            TileMapPathMap = worldSettings.ToDictionary(x => x.Name, x => x);
            MarkerToggleViewModel = new MarkerToggleViewModel(_map);

            QuestListViewModel = new QuestListViewModel(questListProvider.GetAllQuests(), availabilityProvider, levelProvider, policyStore);
            QuestListViewModel.ItemSelectedChanged += QuestListViewModel_ItemSelectedChanged;
            QuestListViewModel.SelectBest();
            gwentStatusProvider.StatusUpdated += GwentStatusProvider_StatusUpdated;
        }

        public ICommand IncreaseLevelCommand => new DelegateCommand(IncreaseLevel);
        private void IncreaseLevel()
        {
            levelProvider.SetLevel(levelProvider.GetLevel() + 1);
            OnPropertyChanged(nameof(PlayerLevel));
        }

        public ICommand DecreaseLevelCommand => new DelegateCommand(DecreaseLevel);
        private void DecreaseLevel()
        {
            int level = levelProvider.GetLevel();
            if (level == 1) { return; }
            levelProvider.SetLevel(level - 1);
            OnPropertyChanged(nameof(PlayerLevel));
        }

        public string PlayerLevel => levelProvider.GetLevel().ToString();

        private void GwentStatusProvider_StatusUpdated()
        {
            OnPropertyChanged(nameof(GwentMessage));
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
        public ICommand OpenGwentWindowCommand => new DelegateCommand(LaunchGwentWindow);

        public ICommand OpenOptionsWindowCommand => new DelegateCommand(LaunchOptionsWindow);

        private void LaunchOptionsWindow()
        {
            optionsDialogWindow.ShowDialog();
        }

        private void LaunchGwentWindow()
        {
            gwentTrackerWindow.LaunchWindow(gwentCardProvider.GetGwentCards(), gwentStatusProvider);
        }

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
