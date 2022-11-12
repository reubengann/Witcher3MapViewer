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
            QuestAvailabilityProvider availabilityProvider = new QuestAvailabilityProvider();
            mainWindow.DataContext = new MainWindowViewModel(mapsUIMap, markerProvider, mapSettingsProvider, questListProvider, availabilityProvider);
            mainWindow.Show();
        }
    }
}
