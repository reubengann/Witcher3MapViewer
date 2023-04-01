using System.IO;
using System.Windows;
using Witcher3MapViewer.Core;

namespace Witcher3MapViewer.WPF
{
    public partial class App : Application
    {
        private const string OptionsPath = "options.json";

        public App()
        {
            XMLMapSettingsProvider mapSettingsProvider = XMLMapSettingsProvider.FromFile("Settings.xml");
            XMLMarkerProvider markerProvider = XMLMarkerProvider.FromFile("MapPins.xml", mapSettingsProvider);
            MainWindow mainWindow = new MainWindow();
            string largeIconPath = mapSettingsProvider.GetIconSettings().LargeIconPath;
            MapsUIMap mapsUIMap = new MapsUIMap(mainWindow.MapControl, Path.Combine(largeIconPath, "Circle.png"));
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
                options.Save("options.json");
            }
            else
            {
                options = Options.FromFile(OptionsPath);
            }


            XMLQuestListProvider questListProvider = XMLQuestListProvider.FromFile("Quests.xml");

            IQuestAvailabilityProvider availabilityProvider;
            ILevelProvider levelProvider;
            IGwentStatusProvider gwentStatusProvider;
            if (options.TrackingMode == TrackingMode.Automatic)
            {
                SaveFileAvailabilityProvider saveFileAvailabilityProvider = new SaveFileAvailabilityProvider(
                                        options.SaveFilePath, "quest_statuses.json", Path.GetTempPath()
                                    );
                availabilityProvider = saveFileAvailabilityProvider;
                levelProvider = new SaveFileLevelProvider(saveFileAvailabilityProvider);
                gwentStatusProvider = new SaveFileGwentStatusProvider(saveFileAvailabilityProvider);
            }
            else
            {
                availabilityProvider = new JsonManualQuestAvailabilityProvider("quest_statuses.json");
                levelProvider = new ManualLevelProvider("level.json");
                gwentStatusProvider = new ManualGwentProvider("gwent_statuses.json");
            }
            OptionsStore optionsStore = new OptionsStore(options, availabilityProvider);
            XMLGwentCardProvider gwentCardProvider = XMLGwentCardProvider.FromFile("Gwent.xml");
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
