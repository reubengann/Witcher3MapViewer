using System.Text.Json;

namespace Witcher3MapViewer.Core
{
    public class OptionsStore
    {
        private Options options;
        private readonly IQuestAvailabilityProvider _availabilityProvider;

        public Options Options { get { return options; } set { options = value; } }

        public OptionsStore(Options options, IQuestAvailabilityProvider availabilityProvider)
        {
            this.options = options;
            _availabilityProvider = availabilityProvider;
        }

        public bool QuestDisplayPolicy(Quest q)
        {
            if (options.ShowOnlyAvailable && !_availabilityProvider.IsQuestAvailable(q))
            {
                return false;
            }
            if (!options.ShowComplete && _availabilityProvider.GetState(q.GUID) == QuestStatusState.Success)
            {
                return false;
            }
            if (q.HideIfAny != null && q.HideIfAny.AllConditions.Count > 0)
            {
                foreach (string c in q.HideIfAny.AllConditions)
                {
                    if (_availabilityProvider.GetState(c) >= QuestStatusState.Active) return false;
                }
            }
            if (q.QuestType == QuestType.Race && !options.ShowRaces) return false;
            if (q.QuestType == QuestType.Treasure && !options.ShowTreasure) return false;
            if (q.QuestType == QuestType.Event && !options.ShowEvents) return false;
            return true;
        }

        public bool IsManual => options.TrackingMode == TrackingMode.Manual;
    }

    public enum TrackingMode
    {
        Manual, Automatic
    }

    public class Options
    {
        public bool ShowOnlyAvailable { get; set; }
        public bool ShowComplete { get; set; }
        public bool ShowRaces { get; set; }
        public bool ShowEvents { get; set; }
        public bool ShowTreasure { get; set; }
        public TrackingMode TrackingMode { get; set; }
        public string SaveFilePath { get; set; }

        public Options(bool showOnlyAvailable, bool showComplete, TrackingMode trackingMode, string saveFilePath, bool showRaces, bool showEvents, bool showTreasure)
        {
            ShowOnlyAvailable = showOnlyAvailable;
            ShowComplete = showComplete;
            TrackingMode = trackingMode;
            SaveFilePath = saveFilePath;
            ShowRaces = showRaces;
            ShowEvents = showEvents;
            ShowTreasure = showTreasure;
        }

        public void Save(string path)
        {
            var file = new OptionsFile
            {
                ShowOnlyAvailable = ShowOnlyAvailable,
                ShowComplete = ShowComplete,
                TrackingMode = TrackingMode == TrackingMode.Automatic ? "Automatic" : "Manual",
                SaveFilePath = SaveFilePath,
                ShowRaces = ShowRaces,
                ShowEvents = ShowEvents,
                ShowTreasure = ShowTreasure
            };
            File.WriteAllText(path, JsonSerializer.Serialize(file, new JsonSerializerOptions { WriteIndented = true }));
        }

        public Options Copy()
        {
            return new Options(ShowOnlyAvailable, ShowComplete, TrackingMode, SaveFilePath, ShowRaces, ShowEvents, ShowTreasure);
        }

        public static Options FromFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }
            var jsonString = File.ReadAllText(path);
            var file = JsonSerializer.Deserialize<OptionsFile>(jsonString);
            if (file == null) throw new Exception("Could not load options from file");
            TrackingMode trackingMode;
            if (file.TrackingMode == "Manual") trackingMode = TrackingMode.Manual;
            else if (file.TrackingMode == "Automatic") trackingMode = TrackingMode.Automatic;
            else { throw new Exception("Unknown tracking mode in file"); }
            return new Options(file.ShowOnlyAvailable, file.ShowComplete, trackingMode, file.SaveFilePath, file.ShowRaces, file.ShowEvents, file.ShowTreasure);
        }

        public static Options Default()
        {
            return new Options(true, false, TrackingMode.Manual, "", true, true, true);
        }
    }

    public class OptionsFile
    {
        public bool ShowOnlyAvailable { get; set; } = true;
        public bool ShowComplete { get; set; } = false;
        public bool ShowRaces { get; set; } = true;
        public bool ShowEvents { get; set; } = true;
        public bool ShowTreasure { get; set; } = true;

        public string TrackingMode { get; set; } = "Manual";
        public string SaveFilePath { get; set; } = "";
    }
}
