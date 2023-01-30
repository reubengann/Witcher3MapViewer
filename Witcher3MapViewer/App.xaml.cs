using System;
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
        public App()
        {
            MainWindow mainWindow = new MainWindow();
            MapsUIMap mapsUIMap = new MapsUIMap(mainWindow.MapControl);
            XMLMapSettingsProvider mapSettingsProvider = XMLMapSettingsProvider.FromFile("Settings.xml");
            XMLMarkerProvider markerProvider = XMLMarkerProvider.FromFile("MapPins.xml", mapSettingsProvider);
            XMLQuestListProvider questListProvider = XMLQuestListProvider.FromFile("Quests.xml");
            string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string ersatzpath = Path.Combine(myDocuments, "The Witcher 3", "gamesaves");
            SaveFileAvailabilityProvider availabilityProvider = new SaveFileAvailabilityProvider(
                    ersatzpath, "", Path.GetTempPath()
                );
            SaveFileLevelProvider levelProvider = new SaveFileLevelProvider(availabilityProvider);
            //ManualQuestAvailabilityProvider availabilityProvider = new ManualQuestAvailabilityProvider();
            mainWindow.DataContext = new MainWindowViewModel(mapsUIMap, markerProvider, mapSettingsProvider, questListProvider, availabilityProvider, levelProvider);
            mainWindow.Show();
        }
    }
}
