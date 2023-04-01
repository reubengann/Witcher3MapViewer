using System.IO;
using System.Windows;
using Witcher3MapViewer.Core;

namespace Witcher3MapViewer.WPF
{
    public partial class App : Application
    {
        private const string OptionsPath = "options.json";
        private const string SettingsXMLPath = "Settings.xml";
        private const string MapPinsXMLPath = "MapPins.xml";
        private const string QuestsXMLPath = "Quests.xml";
        private const string CirclePNGPath = "Circle.png";
        private const string OptionsJSONPath = "options.json";
        private const string QuestStatusJsonPath = "quest_statuses.json";
        private const string ManualLevelJsonPath = "level.json";
        private const string ManualGwentJsonPath = "gwent_statuses.json";
        private const string GwentXMLPath = "Gwent.xml";

        public App()
        {
            XMLMapSettingsProvider mapSettingsProvider = XMLMapSettingsProvider.FromFile(SettingsXMLPath);
            XMLMarkerProvider markerProvider = XMLMarkerProvider.FromFile(MapPinsXMLPath, mapSettingsProvider);
            MainWindow mainWindow = new MainWindow();
            string largeIconPath = mapSettingsProvider.GetIconSettings().LargeIconPath;
            MapsUIMap mapsUIMap = new MapsUIMap(mainWindow.MapControl, Path.Combine(largeIconPath, CirclePNGPath));
            mainWindow.Show();

            Options options;
            if (!File.Exists(OptionsPath))
            {
                options = Options.Default();
                OptionsWPFDialogWindow optionsWPFDialogWindow = new OptionsWPFDialogWindow(options);
                var accepted = optionsWPFDialogWindow.ShowDialog();
                if (!accepted)
                {
                    mainWindow.Close();
                    return;
                };
                options.Save(OptionsJSONPath);
            }
            else
            {
                options = Options.FromFile(OptionsPath);
            }


            XMLQuestListProvider questListProvider = XMLQuestListProvider.FromFile(QuestsXMLPath);

            IQuestAvailabilityProvider availabilityProvider;
            ILevelProvider levelProvider;
            IGwentStatusProvider gwentStatusProvider;
            if (options.TrackingMode == TrackingMode.Automatic)
            {
                SaveFileAvailabilityProvider saveFileAvailabilityProvider = new SaveFileAvailabilityProvider(
                                        options.SaveFilePath, QuestStatusJsonPath, Path.GetTempPath()
                                    );
                availabilityProvider = saveFileAvailabilityProvider;
                levelProvider = new SaveFileLevelProvider(saveFileAvailabilityProvider);
                gwentStatusProvider = new SaveFileGwentStatusProvider(saveFileAvailabilityProvider);
            }
            else
            {
                availabilityProvider = new JsonManualQuestAvailabilityProvider(QuestStatusJsonPath);
                levelProvider = new ManualLevelProvider(ManualLevelJsonPath);
                gwentStatusProvider = new ManualGwentProvider(ManualGwentJsonPath);
            }
            OptionsStore optionsStore = new OptionsStore(options, availabilityProvider);
            XMLGwentCardProvider gwentCardProvider = XMLGwentCardProvider.FromFile(GwentXMLPath);
            GwentTrackerWPFWindow gwentTrackerWPFWindow = new GwentTrackerWPFWindow();

            mainWindow.DataContext = new MainWindowViewModel(
                mapsUIMap,
                markerProvider,
                mapSettingsProvider,
                questListProvider,
                availabilityProvider,
                gwentCardProvider,
                levelProvider,
                gwentStatusProvider,
                gwentTrackerWPFWindow,
                new OptionsWPFDialogWindow(options),
                optionsStore);
        }
    }
}
