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
            XMLMarkerProvider markerProvider = new XMLMarkerProvider();
            mainWindow.DataContext = new MainWindowViewModel(mapsUIMap, markerProvider);
            mainWindow.Show();
        }
    }
}
