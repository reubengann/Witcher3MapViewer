using System.Collections.ObjectModel;

namespace Witcher3MapViewer.Core
{
    public class MarkerToggleViewModel : BaseViewModel
    {
        string appdir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public ObservableCollection<MarkerToggleItemViewModel> Markers { get; set; }
        public MarkerToggleViewModel()
        {
            Markers = new ObservableCollection<MarkerToggleItemViewModel>
            {
                new MarkerToggleItemViewModel("All Items", Path.Combine(appdir, "SmallMarkerImages", "alchemy.png"), null)
            };
            Markers[0].Children = new ObservableCollection<MarkerToggleItemViewModel>()
            {
                new MarkerToggleItemViewModel
                (
                    "Item 1",
                    Path.Combine(appdir, "SmallMarkerImages", "Armorer.png"),
                    Markers[0]
                ),
                new MarkerToggleItemViewModel
                (
                    "Item 2",
                    Path.Combine(appdir, "SmallMarkerImages", "BanditCampfire.png"),
                    Markers[0]
                )
            };
        }
    }
}