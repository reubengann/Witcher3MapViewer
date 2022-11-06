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
            XMLMapSettingsProvider mapSettingsProvider;
            using (FileStream sr = new FileStream(@"C:\repos\Witcher3MapViewer\Witcher3MapViewer\Settings.xml", FileMode.Open))
            {
                mapSettingsProvider = new XMLMapSettingsProvider(sr);
            }
            XMLMarkerProvider markerProvider;
            using (FileStream sr = new FileStream(@"C:\repos\Witcher3MapViewer\Witcher3MapViewer\MapPins.xml", FileMode.Open))
            {
                markerProvider = new XMLMarkerProvider(sr, mapSettingsProvider);
            }
            mainWindow.DataContext = new MainWindowViewModel(mapsUIMap, markerProvider);
            mainWindow.Show();
        }
    }
}
