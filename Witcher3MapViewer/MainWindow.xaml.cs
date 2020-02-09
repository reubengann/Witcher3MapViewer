using System.Windows;
using SQLite;
using BruTile;
using System.Collections.Generic;
using System.Collections.Specialized;
using Mapsui.Styles;
using System.IO;
using System.Threading;
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Witcher3MapViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    /*
     * Known Issues: 
     *  Gwent window showing random players does not update the treeview
     *  Visibility of info window binding does not appear to work
     * 
     * TODO:               
     *  Gwent window highlight should show on map (this is kind of a lot of work)
     *  Gwent players should be circles around regular mappins?
     *  Is there a way to tell whether a random gwent player has been played? Probably not easy
     *  Add ability for multiple points of discovery
     *  Debug manual mode and switching between modes
     *  DLC gwent? Not really necessary     
     *  Save gwent info in manual mode
     *  De-uglify some of the icons
     *  Shift SetIfUndone to the viewmodel
    */

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string appdir;
        private Dictionary<string, RealToGameSpaceConversion> ConversionLibrary;
        private Dictionary<string, MarkerViewModel> MarkersPerWorld;
        private Dictionary<string, string> PathLibrary;
        private string currentLocation, circleShownOnLocation;
        MapPinReader mappindata;
        Dictionary<string, string> ShortNameLookup; //needed because of aliases
        List<string> ShortNames;
        string largeiconpath;
        string smalliconpath;
        StringDictionary MarkersState;
        Dictionary<string, int> IconCache = new Dictionary<string, int>();
        ManualResetEvent _refreshRequested = new ManualResetEvent(false);
        bool MapChanged = false, QuestChanged = false, SaveChanged = false, autosave = false;
        RealToGameSpaceConversion CurrentConvertor;
        Witcher3ProgressStatus progressStatus;
        Witcher3SaveFile ActiveSave;
        string ActiveSaveFileCopy;
        Mapsui.Layers.MemoryLayer CircleLayer;
        Mapsui.Layers.TileLayerAbort tileLayer;
        FileSystemWatcher _fileSystemWatcher;
        QuestViewModel _currentSelection;
        List<Witcher3GwentCard> BaseGameGwentCards;
        GwentTracker activeGwentTrackerWindow;
        bool RandomPlayersOnMap = false;
        bool skipnextrecenter = false;

        //Applications settings
        private bool showAccessibleOnly, showEvents, showRaces, showTreasure, manualMode;
        private string SaveFolder;


        public event PropertyChangedEventHandler PropertyChanged;

        ObservableCollection<QuestViewModel> _currentQuests;
        public ObservableCollection<QuestViewModel> CurrentQuests { get { return _currentQuests; } }

        ObservableCollection<MarkerViewModel> _markers;
        public ObservableCollection<MarkerViewModel> Markers { get { return _markers; } set { _markers = value; } }

        private int _playerlevel = 1;
        public int PlayerLevel
        {
            get { return _playerlevel; }
            set
            {
                _playerlevel = value;
                Level_textbox.Text = _playerlevel.ToString();
                RaisePropertyChanged("PlayerLevel");
            }
        }

        private int _worldSelectIndex;
        public int WorldSelectIndex
        {
            get { return _worldSelectIndex; }
            set
            {
                _worldSelectIndex = value;
                LoadMap(ShortNames[_worldSelectIndex]);
                //we don't want to show the current circles on other maps
                if (currentLocation == circleShownOnLocation && CircleLayer != null)
                    CircleLayer.Enabled = true;
                else CircleLayer.Enabled = false;

                RaisePropertyChanged("WorldSelectIndex");
            }
        }

        private List<System.Windows.Controls.ComboBoxItem> _worldSelectList;
        public List<System.Windows.Controls.ComboBoxItem> WorldSelectList
        {
            get { return _worldSelectList; }
            set
            {
                _worldSelectList = value;
                RaisePropertyChanged("WorldSelectList");
            }
        }



        private string _infoMessage;
        public string InfoMessage
        {
            get
            {
                return _infoMessage;
            }
            set
            {
                _infoMessage = value;
                if (_infoMessage == "")
                    InfoShown = false;
                else InfoShown = true;

                RaisePropertyChanged("InfoMessage");
            }
        }

        private bool _infoShown;
        public bool InfoShown
        {
            get { return _infoShown; }
            set { _infoShown = value; RaisePropertyChanged("InfoShown"); }
        }

        string _gwentStatusText = "This is the status";
        public string GwentStatusText
        {
            get
            {
                return _gwentStatusText;
            }
            set
            {
                _gwentStatusText = value;
                RaisePropertyChanged("GwentStatusText");
            }
        }

        string _searchQuery = "";
        public string SearchQuery
        {
            get { return _searchQuery; }
            set { _searchQuery = value; UpdateSearch(); }
        }

        bool _searchShown = false;
        public bool SearchShown { get { return _searchShown; } set { _searchShown = value; RaisePropertyChanged("SearchShown"); } }

        public MainWindow()
        {
            //suppresses pesky debug errors about invalid ancestor to combobox
#if DEBUG
            System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
#endif

            int localplayerlevel = 1;
            DataSetup();
            if (manualMode && File.Exists(Path.Combine(appdir, "status.dat")))
            {
                Readers.GameStatusReader statusreader = new Readers.GameStatusReader(Path.Combine(appdir, "status.dat"));
                if (statusreader.QuestInfo != null && statusreader.QuestInfo.Count != 0)
                {
                    MessageBoxResult result =
                    MessageBox.Show("Load quest info from previous session?", "Load data?", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        ProcessLastStatus(statusreader.QuestInfo);
                        localplayerlevel = statusreader.PlayerLevel;
                    }
                }

            }
            DataContext = this;
            InitializeComponent();
            PlayerLevel = localplayerlevel;

            if (Properties.Settings.Default.FirstRun)
            {
                Properties.Settings.Default.FirstRun = false;
                var foo = new SettingsDialog();
                foo.ShowDialog();
            }
            SettingsFromConfig();

            _currentQuests = new ObservableCollection<QuestViewModel>();
            if (!manualMode)
            {
                if (Directory.Exists(SaveFolder))
                {
                    GetMostRecentSaveFile(); //automatically calls updatequests
                    SetUpFilewatcher();
                }
                else
                {
                    MessageBox.Show("The tracker has been put into manual mode. If you want to track quests" +
                        " automatically, choose a valid folder in the settings.");
                    manualMode = true;
                    Properties.Settings.Default.ManualMode = true;
                }
            }
            else
            {
                GwentStatusText = "Click to see Gwent cards";
                UpdateQuests();
            }
            _currentQuests.Sort();
            RaisePropertyChanged("CurrentQuests");

            InfoMessage = "";

            System.Windows.Threading.DispatcherTimer autosaveTimer = new System.Windows.Threading.DispatcherTimer(
            TimeSpan.FromSeconds(120), System.Windows.Threading.DispatcherPriority.Background,
            new EventHandler(DoAutoSave), Application.Current.Dispatcher);
            autosaveTimer.Start();

            SeekNextUndone();
            MyMapControl.Map.Viewport.Resolution = 20000;

            SearchBox.NextItemButton.Click += NextItemButton_Click;
            SearchBox.PrevItemButton.Click += PrevItemButton_Click;
            SearchBox.CloseSearchButton.Click += CloseSearchButton_Click;
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Find, FindExecuted, FindCanExecute));
        }

        private void FindCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !SearchShown;
        }

        private void FindExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SearchShown = true;
            SearchBox.QueryText.Focus();
            Keyboard.Focus(SearchBox.QueryText);
        }

        private void UpdateSearch()
        {
            SearchQuests(SearchQuery, 0, SearchDirection.Forward);
        }

        private void PrevItemButton_Click(object sender, RoutedEventArgs e)
        {
            int start = 0;
            if (_currentSelection != null)
                start = CurrentQuests.IndexOf(_currentSelection);

            SearchQuests(SearchQuery, start, SearchDirection.Backward);

        }

        private void NextItemButton_Click(object sender, RoutedEventArgs e)
        {
            int start = 0;
            if (_currentSelection != null)
                start = CurrentQuests.IndexOf(_currentSelection);

            SearchQuests(SearchQuery, start, SearchDirection.Forward);
        }

        private enum SearchDirection { Forward, Backward }

        private void SearchQuests(string query, int start, SearchDirection direction)
        {
            if (CurrentQuests == null || CurrentQuests.Count == 0)
                return;
            if (query == "") return;
            int increment = 1;
            if (direction == SearchDirection.Backward)
                increment = -1;
            int current = start + increment;
            int count = CurrentQuests.Count;
            while (current != start)
            {
                QuestViewModel qvm = CurrentQuests[current];
                if (qvm.Name.CaseInsensitiveContains(SearchQuery))
                {
                    qvm.IsSelected = true;
                    _currentSelection = qvm;
                    return;
                }
                current += increment;
                if (current == count)
                    current = 0;
                if (current < 0)
                    current = count - 1;
            }
        }

        private void CloseSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchShown = false;
        }

        private void ProcessLastStatus(List<Quest> lastStatus)
        {
            //Go through the quests and set the flags according to the status.dat file.            
            bool foundproblem = false;
            foreach (Quest q in lastStatus)
            {
                Quest questtoupdate = progressStatus.Quests.Where(item => item.UniqueID == q.UniqueID).First();

                if (questtoupdate.GUID.Value != q.GUID.Value)
                {
                    foundproblem = true;
                    continue;
                }
                if (q.Done)
                {
                    questtoupdate.Done = true;
                    questtoupdate.Forced = true;
                    questtoupdate.Status = QuestStatusState.Success;
                }
                if (q.Deferred)
                    questtoupdate.Deferred = true;

            }
            if (foundproblem)
            {
                MessageBox.Show("The data from a previous run is inconsistent with the current xml file. Previous manual statuses have been" +
                        " restored where possible");
            }
        }

        private void SetUpFilewatcher()
        {
            _fileSystemWatcher = new FileSystemWatcher(SaveFolder);
            _fileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            _fileSystemWatcher.Filter = "*.sav";
            _fileSystemWatcher.Changed += _fileSystemWatcher_Changed;
            _fileSystemWatcher.Created += _fileSystemWatcher_Changed;
            _fileSystemWatcher.Renamed += _fileSystemWatcher_Changed;
            _fileSystemWatcher.EnableRaisingEvents = true;
        }



        private void CanFindNext(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void _fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(2000);
            SaveChanged = true;
            _refreshRequested.Set();
            ThreadPool.QueueUserWorkItem(RefreshIfNecessary);
        }

        private void DataSetup()
        {
            appdir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!Directory.Exists(Path.Combine(appdir, "SavedGames")))
                Directory.CreateDirectory(Path.Combine(appdir, "SavedGames"));

            MarkersPerWorld = new Dictionary<string, MarkerViewModel>();
            Readers.ApplicationSettingsReader Settings = new Readers.ApplicationSettingsReader(appdir, "Settings.xml");
            ConversionLibrary = Settings.ConversionLibrary;
            PathLibrary = Settings.PathLibrary;
            ShortNameLookup = Settings.ShortNameLookup;
            ShortNames = Settings.ShortNames;
            largeiconpath = Settings.largeiconpath;
            smalliconpath = Settings.smalliconpath;

            List<System.Windows.Controls.ComboBoxItem> source = new List<System.Windows.Controls.ComboBoxItem>();
            foreach (string longname in Settings.LongNames)
            {
                source.Add(new System.Windows.Controls.ComboBoxItem { Content = longname });
            }
            WorldSelectList = source;
            mappindata = new MapPinReader(Path.Combine(appdir, "MapPins.xml"), Settings.TypeLookup);
            GetGwentCardList();
            LoadQuestData();
        }

        private void GetGwentCardList()
        {
            var reader = new Readers.GwentXMLReader(Path.Combine(appdir, "Gwent.xml"));

            BaseGameGwentCards = new List<Witcher3GwentCard>();
            foreach (Readers.GwentCardAsRead cardasread in reader.Sets[0].Cards)
            {
                Witcher3GwentCard card = new Witcher3GwentCard();
                card.cardIndex = cardasread.ID;
                card.Name = cardasread.Name;
                card.Location = cardasread.Location;
                card.AssociatedQuest = cardasread.AssociatedQuest;
                BaseGameGwentCards.Add(card);
            }
        }

        private void comboBox_Loaded(object sender, RoutedEventArgs e)
        {
            //Make combobox open upwards
            System.Windows.Controls.ControlTemplate ct = comboBox.Template;
            System.Windows.Controls.Primitives.Popup pop = ct.FindName("PART_Popup", comboBox) as System.Windows.Controls.Primitives.Popup;
            pop.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
        }

        private void LoadMap(string location)
        {
            ObservableCollection<MarkerViewModel> tempmarkers = new ObservableCollection<MarkerViewModel>();

            MarkerViewModel root;
            bool worldWasAlreadyLoaded = MarkersPerWorld.ContainsKey(location);
            if (worldWasAlreadyLoaded)
            {
                root = MarkersPerWorld[location];
            }
            else
            {
                root = MarkerViewModel.CreateRoot(Path.Combine(appdir, smalliconpath));
                MarkersPerWorld.Add(location, root);
            }

            currentLocation = location;
            CurrentConvertor = ConversionLibrary[location];
            if (tileLayer != null)
            {
                //fixes memory leak in Mapsui
                tileLayer.AbortFetch();
                MyMapControl.Map.Layers.Clear();
            }

            MbTilesTileSource _source = new MbTilesTileSource(new SQLiteConnectionString(PathLibrary[location], false));
            tileLayer = new Mapsui.Layers.TileLayerAbort(_source);
            MyMapControl.Map.Layers.Add(tileLayer);
            Dictionary<MapPinType, MapPinCollection> worldpindata = mappindata.GetPins(ShortNameLookup[location]);
            foreach (MapPinType pintype in worldpindata.Keys)
            {
                //I want to do this last
                if (pintype.InternalName == "RoadSign")
                    continue;

                if (worldWasAlreadyLoaded)
                {
                    MyMapControl.Map.Layers.Add(root.FindChild(pintype.InternalName).AssociatedLayer);
                    continue;
                }

                Mapsui.Layers.MemoryLayer _pointlayer = new Mapsui.Layers.MemoryLayer { Style = null };
                List<Mapsui.Geometries.Point> _points = new List<Mapsui.Geometries.Point>();
                string path = Path.Combine(appdir, largeiconpath, pintype.IconFile);

                //Load the image if needed
                if (!IconCache.ContainsKey(path))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        MemoryStream ms = new MemoryStream();
                        fs.CopyTo(ms);
                        int id = BitmapRegistry.Instance.Register(ms);
                        IconCache[path] = id;
                    }
                }

                if (pintype.InternalName.Contains("Quest"))
                {
                    foreach (MapPin c in worldpindata[pintype].Locations)
                    {
                        //Try to match Pin coords and Quest coords
                        Quest q = progressStatus.getQuestByCoordinates(Convert.ToInt32(c.Location.X), Convert.ToInt32(c.Location.Y));
                        //If no matching quest found then still display the Pin
                        bool questNotDoneOrNotFound = q == null || !q.Done;
                        if (questNotDoneOrNotFound)
                            _points.Add(CurrentConvertor.ToWorldSpace(c.Location));
                    }
                }
                else
                    foreach (MapPin c in worldpindata[pintype].Locations)
                        _points.Add(CurrentConvertor.ToWorldSpace(c.Location));

                _pointlayer.DataSource = new Mapsui.Providers.MemoryProvider(_points);

                SymbolStyle _style = new SymbolStyle();


                _style.BitmapId = IconCache[path];
                _pointlayer.Style = _style;

                _pointlayer.Name = pintype.Name;
                MyMapControl.Map.Layers.Add(_pointlayer);
                root.AddChild(new MarkerViewModel(pintype, _pointlayer));
            }
            root.VerifyCheckState();
            AddCircleLayer();
            //Add label layer last so that it's on top
            MyMapControl.Map.Layers.Add(GetLabelLayer(worldpindata));
            MyMapControl.Map.BackColor = new Color(184, 222, 230);
            tempmarkers.Add(root);
            _markers = tempmarkers;
            RaisePropertyChanged("Markers");
            if (RandomPlayersOnMap)
                ToggleGwentPlayers(true);
            else
            {
                MarkerViewModel gw = Markers[0].FindChild("GwentPlayer");
                if (gw != null) gw.IsChecked = false;
            }

            MyMapControl.Refresh();
        }

        private void ToggleGwentPlayers(bool ShowLayer)
        {
            Markers[0].IsChecked = !ShowLayer; //turn off all
            MarkerViewModel gw = Markers[0].FindChild("GwentPlayer");
            if (gw != null) gw.IsChecked = ShowLayer; //enable only gwent players                        
            RandomPlayersOnMap = ShowLayer;
        }

        private Mapsui.Layers.MemoryLayer GetLabelLayer(Dictionary<MapPinType, MapPinCollection> worldpindata)
        {
            Mapsui.Layers.MemoryLayer _pointlayer = new Mapsui.Layers.MemoryLayer { Style = null };
            List<Mapsui.Geometries.Point> _points = new List<Mapsui.Geometries.Point>();
            MapPinType pintype = worldpindata.Keys.ToList().Where(item => item.InternalName == "RoadSign").FirstOrDefault();
            string path = Path.Combine(appdir, largeiconpath, pintype.IconFile);
            var memoryProvider = new Mapsui.Providers.MemoryProvider();
            //Load the image if needed
            if (!IconCache.ContainsKey(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    MemoryStream ms = new MemoryStream();
                    fs.CopyTo(ms);
                    int id = BitmapRegistry.Instance.Register(ms);
                    IconCache[path] = id;
                }
            }

            foreach (MapPin c in worldpindata[pintype].Locations)
            {
                var feature = new Mapsui.Providers.Feature
                {
                    Geometry = CurrentConvertor.ToWorldSpace(c.Location),
                };
                feature.Styles.Add(new LabelStyle
                {
                    Text = BreakLines(c.Name, 17),
                    ForeColor = Color.White,
                    BackColor = new Brush(Color.Gray),
                    Halo = new Pen(Color.Black, 2),
                    VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom,
                    Offset = new Offset(0, 14),
                    CollisionDetection = false
                });
                feature.Styles.Add(new SymbolStyle { BitmapId = IconCache[path] });
                memoryProvider.Features.Add(feature);
            }
            _pointlayer.DataSource = memoryProvider;
            return _pointlayer;
        }

        private string BreakLines(string tobreak, int desiredmaxchars)
        {
            if (!tobreak.Contains(" ")) return tobreak;
            string[] words = tobreak.Split(' ');
            string broken = words[0];
            int count = broken.Length;
            for (int i = 1; i < words.Length; i++)
            {
                if (count + words[i].Length > desiredmaxchars)
                {
                    broken += "\n";
                    count = 0;
                }
                else
                {
                    broken += " ";
                    count++;
                }
                broken += words[i];
                count += words[i].Length;
            }
            return broken;
        }

        private void AddCircleLayer()
        {
            if (CircleLayer == null)
            {
                Mapsui.Layers.MemoryLayer _pointlayer = new Mapsui.Layers.MemoryLayer();
                List<Mapsui.Geometries.Point> _points = new List<Mapsui.Geometries.Point>();
                _pointlayer.DataSource = new Mapsui.Providers.MemoryProvider(_points);
                _pointlayer.Name = "Circle";
                SymbolStyle _style = new SymbolStyle();
                string path = Path.Combine(appdir, largeiconpath, Path.Combine(appdir, @"MarkerImages\Circle.png"));
                if (!IconCache.ContainsKey(path))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        MemoryStream ms = new MemoryStream();
                        fs.CopyTo(ms);
                        int id = BitmapRegistry.Instance.Register(ms);
                        IconCache[path] = id;
                    }
                }
                _style.BitmapId = IconCache[path];
                _pointlayer.Style = _style;
                CircleLayer = _pointlayer;
            }
            MyMapControl.Map.Layers.Add(CircleLayer);
        }

        private void AddCircleAt(Mapsui.Geometries.Point Center)
        {
            circleShownOnLocation = currentLocation;
            CircleLayer.DataSource = new Mapsui.Providers.MemoryProvider(CurrentConvertor.ToWorldSpace(Center));
            CircleLayer.Enabled = true;
        }

        private void ToggleLayers()
        {
            MyMapControl.Refresh();
        }

        //Check a marker checkbox
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

            MapChanged = true;
            _refreshRequested.Set();
            ThreadPool.QueueUserWorkItem(RefreshIfNecessary);
        }

        private void RefreshIfNecessary(object state)
        {
            lock (this)
            {
                if (!_refreshRequested.WaitOne(0))
                {
                    // Refresh not necessary
                    return;
                }
                if (MapChanged)
                {
                    ToggleLayers();
                }
                if (QuestChanged)
                {
                    //needed because the _quests collection created in UI thread
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        Dispatcher.BeginInvoke(new Action(UpdateQuests));
                    });
                }
                if (SaveChanged)
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        Dispatcher.BeginInvoke(new Action(ExecuteSaveChange));
                    });
                }
                if (manualMode && autosave)
                    Readers.GameStatusSaver.Save("status.dat", _currentQuests.ToList(), PlayerLevel);

                // Clear all flags
                MapChanged = false;
                QuestChanged = false;
                SaveChanged = false;
                autosave = false;
                _refreshRequested.Reset();
            }
        }

        private void ExecuteSaveChange()
        {
            ClearLocalSave();
            GetMostRecentSaveFile();
            SeekNextUndone();
        }

        private void ClearLocalSave()
        {
            if (File.Exists(ActiveSaveFileCopy))
                File.Delete(ActiveSaveFileCopy);
        }

        private void UpdateQuests()
        {
            Witcher3ProgressStatus.AvailCond Conditions = FormConditions();
            List<Quest> addedItems = progressStatus.GetAvailable(Conditions);
            foreach (Quest q in addedItems)
                _currentQuests.Add(new QuestViewModel(q));

            List<Quest> removedItems = progressStatus.CullNewlyUnavailable(Conditions);
            if (removedItems.Count != 0)
            {
                foreach (Quest q in removedItems)
                {
                    QuestViewModel goner = _currentQuests.Where(item => item.correspondingQuest == q).First();
                    _currentQuests.Remove(goner);
                }
            }

            //Update checked status from underlying quests
            foreach (QuestViewModel q in _currentQuests)
                q.Recheck();

            _currentQuests.Sort(); //even if none were added, we need to sort done objects to the top, etc.
            if (_currentSelection != null && _currentSelection.IsSelected == false)
            {
                //sorting resets IsSelected field, so reset it.
                skipnextrecenter = true;
                _currentSelection.IsSelected = true;
            }

            if (manualMode)
                SeekNextUndone();
        }

        private Witcher3ProgressStatus.AvailCond FormConditions()
        {
            Witcher3ProgressStatus.AvailCond Conditions = 0;
            if (showAccessibleOnly)
                Conditions = Conditions | Witcher3ProgressStatus.AvailCond.Accessible;
            if (showTreasure)
                Conditions = Conditions | Witcher3ProgressStatus.AvailCond.Treasure;
            if (showRaces)
                Conditions = Conditions | Witcher3ProgressStatus.AvailCond.Races;
            if (showEvents)
                Conditions = Conditions | Witcher3ProgressStatus.AvailCond.Events;
            return Conditions;
        }

        private void MyMapControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point screenPosition = e.GetPosition(MyMapControl);
            Mapsui.Geometries.Point earthPosition = MyMapControl.Map.Viewport.ScreenToWorld(screenPosition.X, screenPosition.Y);
            Mapsui.Geometries.Point worldPosition = CurrentConvertor.ToGameSpace(earthPosition);
            HoverPositionXY.Text = $"{worldPosition.X:F0}, {worldPosition.Y:F0}";
        }

        private void PART_IncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerLevel++;
        }

        private void PART_DecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerLevel > 1)
                PlayerLevel--;
        }

        private void Level_textbox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int i;
            if (int.TryParse(Level_textbox.Text, out i))
            {
                PlayerLevel = i;
            }
            else Level_textbox.Text = PlayerLevel.ToString();
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            QuestChanged = true;
            _refreshRequested.Set();
            ThreadPool.QueueUserWorkItem(RefreshIfNecessary);
        }

        private void questTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (skipnextrecenter)
            {
                skipnextrecenter = false;
                return;
            }
            if (e.NewValue == null)
                return;
            if (e.NewValue.GetType() != typeof(QuestViewModel))
                return;
            Quest selected = ((QuestViewModel)e.NewValue).correspondingQuest;
            if (selected.Deferred)
                questTree.ContextMenu = questTree.Resources["Deferred"] as System.Windows.Controls.ContextMenu;
            else questTree.ContextMenu = questTree.Resources["Undeferred"] as System.Windows.Controls.ContextMenu;


            if (selected.DiscoverPrompt == null)
            {
                if (CircleLayer != null)
                {
                    CircleLayer.Enabled = false;
                    circleShownOnLocation = null;
                }
                if (currentLocation != ShortNameLookup[selected.World])
                {
                    WorldSelectIndex = ShortNames.IndexOf(ShortNameLookup[selected.World]);
                }
                return;
            }
            if (selected.DiscoverPrompt.DiscoverPosition != null)
            {
                if (CircleLayer != null) CircleLayer.Enabled = true;
                if (currentLocation != ShortNameLookup[selected.DiscoverPrompt.DiscoverPosition.World])
                {
                    WorldSelectIndex = ShortNames.IndexOf(ShortNameLookup[selected.DiscoverPrompt.DiscoverPosition.World]);
                }
                CenterMap(selected.DiscoverPrompt.DiscoverPosition.X, selected.DiscoverPrompt.DiscoverPosition.Y);
                AddCircleAt(new Mapsui.Geometries.Point(selected.DiscoverPrompt.DiscoverPosition.X,
                    selected.DiscoverPrompt.DiscoverPosition.Y));
                MyMapControl.Refresh();
            }
            else
            {
                if (CircleLayer != null)
                {
                    CircleLayer.Enabled = false;
                    circleShownOnLocation = null;
                }
            }
            if (selected.DiscoverPrompt.Info != "")
            {
                InfoMessage = selected.DiscoverPrompt.Info;
            }
        }

        private void CenterMap(int x, int y)
        {
            MyMapControl.Map.Viewport.Center = CurrentConvertor.ToWorldSpace(new Mapsui.Geometries.Point(x, y));
        }

        private void LoadQuestData()
        {
            progressStatus = new Witcher3ProgressStatus(Path.Combine(appdir, "Quests.xml"), FormConditions());
        }

        private void RaisePropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void GetMostRecentSaveFile()
        {
            //Get file listing, find most recent file
            DirectoryInfo directory = new DirectoryInfo(SaveFolder);
            var VerifyThereAreSomeFiles = directory.GetFiles("*.sav");
            if (VerifyThereAreSomeFiles == null || VerifyThereAreSomeFiles.Length == 0)
                return;
            FileInfo myFile = (from f in directory.GetFiles("*.sav")
                               orderby f.LastWriteTime descending
                               select f).First();
            //copy to local directory
            if (CopyWhenAvailable(myFile.FullName))
            {
                myFile = new FileInfo(Path.Combine(appdir, "SavedGames", myFile.Name));
                ActiveSaveFileCopy = myFile.FullName;
                ActiveSave = new Witcher3SaveFile(myFile.FullName, Witcher3ReadLevel.Quick);
                progressStatus.PopulateStatusesFromJournal(ActiveSave.CJournalManager);
                PlayerLevel = ActiveSave.CharacterLevel;
                UpdateQuests();
                UpdateGwent();
            }
        }

        private void UpdateGwent()
        {
            List<int> OwnedCards = new List<int>();
            foreach (Witcher3GwentCard thecard in ActiveSave.GwentManager.RegularCards)
                OwnedCards.Add(thecard.cardIndex);
            foreach (Witcher3GwentCard thecard in ActiveSave.GwentManager.LeaderCards)
                OwnedCards.Add(thecard.cardIndex);
            int numowned = 0;
            foreach (Witcher3GwentCard thecard in BaseGameGwentCards)
            {
                if (OwnedCards.Contains(thecard.cardIndex))
                    numowned++;
            }
            float percent = 100 * numowned / BaseGameGwentCards.Count;
            GwentStatusText = "Gwent cards: " + numowned.ToString() + "/" + BaseGameGwentCards.Count.ToString() + " ("
                + percent.ToString("0") + "%)";
            if (activeGwentTrackerWindow != null)
            {
                activeGwentTrackerWindow.UpdateCards(ActiveSave.GwentManager.RegularCards, ActiveSave.GwentManager.LeaderCards);
            }
        }

        private void TheMainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (manualMode)
            {
                Readers.GameStatusSaver.Save("status.dat", _currentQuests.ToList(), PlayerLevel);
                if (activeGwentTrackerWindow != null)
                    activeGwentTrackerWindow.Close();
            }
        }

        private void GwentOpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (activeGwentTrackerWindow == null)
            {
                activeGwentTrackerWindow = new GwentTracker();
                if (manualMode)
                    activeGwentTrackerWindow.LoadInfo(BaseGameGwentCards);
                else
                    activeGwentTrackerWindow.LoadInfo(BaseGameGwentCards, ActiveSave.GwentManager.RegularCards, ActiveSave.GwentManager.LeaderCards);

                activeGwentTrackerWindow.Show();
                activeGwentTrackerWindow.RequestedGwentPlayersOnMap += ActiveGwentTrackerWindow_RequestedGwentPlayersOnMap;
                activeGwentTrackerWindow.Closed += ActiveGwentTrackerWindow_Closed;
            }
        }

        private void ActiveGwentTrackerWindow_RequestedGwentPlayersOnMap(object sender, EventArgs e)
        {
            if (RandomPlayersOnMap)
                ToggleGwentPlayers(false);
            else ToggleGwentPlayers(true);
        }

        private void ActiveGwentTrackerWindow_Closed(object sender, EventArgs e)
        {
            activeGwentTrackerWindow.Closed -= ActiveGwentTrackerWindow_Closed;
            activeGwentTrackerWindow.RequestedGwentPlayersOnMap -= ActiveGwentTrackerWindow_RequestedGwentPlayersOnMap;
            activeGwentTrackerWindow = null;
            if (RandomPlayersOnMap)
                ToggleGwentPlayers(false);
        }

        private void DeferItem_Click(object sender, RoutedEventArgs e)
        {
            QuestViewModel itemtodefer = (QuestViewModel)questTree.SelectedItem;
            itemtodefer.IsDeferred = !itemtodefer.IsDeferred;
            _currentQuests.Sort();
            SeekNextUndone();
        }

        private void SettingsButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowSettings();
        }

        private void ShowSettings()
        {
            bool wasManual = manualMode;
            var foo = new SettingsDialog();
            foo.ShowDialog();
            SettingsFromConfig();
            if (foo.RequestNewGame)
            {
                PlayerLevel = 1;
                _currentQuests.Clear();
                progressStatus.Reset();
            }
            if (wasManual && !manualMode)
            {
                GetMostRecentSaveFile();
                SetUpFilewatcher();
                SeekNextUndone();
            }
            UpdateQuests();
        }

        private void ShowSearch()
        {
            SearchShown = true;
        }

        private bool CopyWhenAvailable(string fullPath)
        {
            int numTries = 0;
            while (true)
            {
                numTries++;
                try
                {
                    File.Copy(fullPath, Path.Combine(appdir, "SavedGames", Path.GetFileName(fullPath)), true);
                    break;
                }
                catch (System.Reflection.AmbiguousMatchException) //this is just so we don't assign the exception to a variable
                {
                    //Failed to get access
                    if (numTries > 10)
                    {
                        //give up
                        return false;
                    }
                    // Wait for the lock to be released
                    Thread.Sleep(500);
                }
            }
            //Success
            return true;
        }

        private void SeekNextUndone()
        {
            // Look for a side quest at or below current level. Otherwise, choose the main quest. If
            // none are available, then it's ok to specify an endgame quest. If all else
            // fails, accept quests that are higher than the player level by a few levels.
            for (int i = 0; i < _currentQuests.Count; i++)
            {
                QuestViewModel qvm = _currentQuests[i];
                if (SetIfUndone(qvm, false, false)
                    && qvm.correspondingQuest.QuestType != "main"
                    && qvm.correspondingQuest.QuestType != "endgame"
                    && qvm.correspondingQuest.LevelRequirement <= PlayerLevel)
                {

                    SetIfUndone(qvm, true, false);
                    return;
                }
            }
            for (int i = 0; i < _currentQuests.Count; i++)
            {
                QuestViewModel qvm = _currentQuests[i];
                if (SetIfUndone(qvm, false, true)
                    && qvm.correspondingQuest.QuestType != "endgame"
                    && qvm.correspondingQuest.LevelRequirement <= PlayerLevel)
                {
                    SetIfUndone(qvm, true, true);
                    if (qvm.correspondingQuest.QuestType == "main")
                    {
                        string tempname;
                        if (_currentSelection._parent != null)
                            tempname = _currentSelection._parent.Name;
                        else tempname = _currentSelection.Name;
                        InfoMessage = "Advance main quest " + tempname;
                    }
                    return;
                }
            }
            int margin = 0;
            while (margin < 4)
            {
                for (int i = 0; i < _currentQuests.Count; i++)
                {
                    QuestViewModel qvm = _currentQuests[i];
                    if ((qvm.correspondingQuest.QuestType == "main"
                        || qvm.correspondingQuest.QuestType == "endgame")
                        && SetIfUndone(qvm, false, true)
                        && qvm.correspondingQuest.LevelRequirement <= PlayerLevel + margin)
                    {
                        SetIfUndone(qvm, true, true);
                        return;
                    }
                }
                margin++;
            }
            InfoMessage = "No undeferred quests found";
        }


        private bool SetIfUndone(QuestViewModel qvm, bool actuallyset, bool greedy)
        {
            if (qvm.IsChecked == true || qvm.IsDeferred == true)
            {
                return false;
            }
            if (qvm.Children.Count == 0 || greedy)
            {
                if (actuallyset)
                {
                    qvm.IsSelected = true;
                    _currentSelection = qvm;
                }
                return true;
            }
            foreach (QuestViewModel qvchild in qvm.Children)
            {
                if (SetIfUndone(qvchild, actuallyset, greedy))
                    return true;
            }
            return false;
        }

        private void SettingsFromConfig()
        {
            SaveFolder = Properties.Settings.Default.SaveFolder;
            manualMode = Properties.Settings.Default.ManualMode;
            showAccessibleOnly = Properties.Settings.Default.hideInaccessible;
            showEvents = Properties.Settings.Default.showEvents;
            showRaces = Properties.Settings.Default.showRaces;
            showTreasure = Properties.Settings.Default.showTreasure;
            MarkersState = Properties.Settings.Default.MarkersState;
            if (MarkersState == null)
                Properties.Settings.Default.MarkersState = new StringDictionary();
        }

        //periodically save user interventions (or manual mode game data)
        private void DoAutoSave(object sender, EventArgs e)
        {
            autosave = true;
            _refreshRequested.Set();
            ThreadPool.QueueUserWorkItem(RefreshIfNecessary);
        }
    }

}
