using System.IO;
using System.Windows;
using Witcher3MapViewer.Core;

namespace Witcher3MapViewer.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string OptionsPath = "options.json";

        public App()
        {
            Options options;
            if (!File.Exists(OptionsPath))
            {
                options = Options.FromFile(OptionsPath); // will create
                OptionsWPFDialogWindow optionsWPFDialogWindow = new OptionsWPFDialogWindow(options);
                var accepted = optionsWPFDialogWindow.ShowDialog();
                if (!accepted)
                {
                    File.Delete(OptionsPath);
                    return;
                };
                options.Save("options.json");
            }
            else
            {
                options = Options.FromFile(OptionsPath);
            }

            //MainWindow mainWindow = new MainWindow();
            //XMLMapSettingsProvider mapSettingsProvider = XMLMapSettingsProvider.FromFile("Settings.xml");
            //XMLMarkerProvider markerProvider = XMLMarkerProvider.FromFile("MapPins.xml", mapSettingsProvider);
            //string largeIconPath = mapSettingsProvider.GetIconSettings().LargeIconPath;
            //MapsUIMap mapsUIMap = new MapsUIMap(mainWindow.MapControl, Path.Combine(largeIconPath, "Circle.png"));
            //XMLQuestListProvider questListProvider = XMLQuestListProvider.FromFile("Quests.xml");
            //string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //string ersatzpath = Path.Combine(myDocuments, "The Witcher 3", "gamesaves");
            //SaveFileAvailabilityProvider availabilityProvider = new SaveFileAvailabilityProvider(
            //        ersatzpath, "quest_statuses.json", Path.GetTempPath()
            //    );
            //OptionsStore optionsStore = new OptionsStore(options, availabilityProvider);
            //SaveFileLevelProvider levelProvider = new SaveFileLevelProvider(availabilityProvider);
            ////ManualQuestAvailabilityProvider availabilityProvider = new ManualQuestAvailabilityProvider();
            //XMLGwentCardProvider gwentCardProvider = XMLGwentCardProvider.FromFile("Gwent.xml");
            //SaveFileGwentStatusProvider saveFileGwent = new SaveFileGwentStatusProvider(availabilityProvider);
            //GwentTrackerWPFWindow gwentTrackerWPFWindow = new GwentTrackerWPFWindow();
            //mainWindow.DataContext = new MainWindowViewModel(mapsUIMap, markerProvider, mapSettingsProvider, questListProvider, availabilityProvider, gwentCardProvider, levelProvider, saveFileGwent, gwentTrackerWPFWindow, optionsWPFDialogWindow, optionsStore);
            //mainWindow.Show();
        }
    }
}
