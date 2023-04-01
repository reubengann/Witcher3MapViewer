using System.Text.Json;

namespace Witcher3MapViewer.Core
{
    public class OptionsStore
    {
        private readonly Options options;
        private readonly IQuestAvailabilityProvider _availabilityProvider;


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
            return true;
        }
    }

    public enum TrackingMode
    {
        Manual, Automatic
    }

    public class Options
    {
        public bool ShowOnlyAvailable { get; set; }
        public bool ShowComplete { get; set; }
        public TrackingMode TrackingMode { get; set; }
        public string SaveFilePath { get; set; }

        public Options(bool showOnlyAvailable, bool showComplete, TrackingMode trackingMode, string saveFilePath)
        {
            ShowOnlyAvailable = showOnlyAvailable;
            ShowComplete = showComplete;
            TrackingMode = trackingMode;
            SaveFilePath = saveFilePath;
        }

        public void Save(string path)
        {
            var file = new OptionsFile
            {
                ShowOnlyAvailable = ShowOnlyAvailable,
                ShowComplete = ShowComplete,
                TrackingMode = TrackingMode == TrackingMode.Automatic ? "Automatic" : "Manual",
                SaveFilePath = SaveFilePath
            };
            File.WriteAllText(path, JsonSerializer.Serialize(file, new JsonSerializerOptions { WriteIndented = true }));
        }

        public static Options FromFile(string path)
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonSerializer.Serialize(new OptionsFile(), new JsonSerializerOptions { WriteIndented = true }));
            }
            var jsonString = File.ReadAllText(path);
            var file = JsonSerializer.Deserialize<OptionsFile>(jsonString);
            if (file == null) throw new Exception("Could not load options from file");
            TrackingMode trackingMode;
            if (file.TrackingMode == "Manual") trackingMode = TrackingMode.Manual;
            else if (file.TrackingMode == "Automatic") trackingMode = TrackingMode.Automatic;
            else { throw new Exception("Unknown tracking mode in file"); }
            return new Options(file.ShowOnlyAvailable, file.ShowComplete, trackingMode, file.SaveFilePath);
        }
    }

    public class OptionsFile
    {
        public bool ShowOnlyAvailable { get; set; } = true;
        public bool ShowComplete { get; set; } = false;
        public string TrackingMode { get; set; } = "Manual";
        public string SaveFilePath { get; set; } = "";
    }
}
