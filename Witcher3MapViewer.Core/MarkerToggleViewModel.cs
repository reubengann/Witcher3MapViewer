using System.Collections.ObjectModel;

namespace Witcher3MapViewer.Core
{
    public class MarkerToggleViewModel : BaseViewModel
    {
        string appdir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        MarkerToggleItemViewModel root;


        public ObservableCollection<MarkerToggleItemViewModel> Markers { get; set; }
        public MarkerToggleViewModel()
        {
            root = new MarkerToggleItemViewModel("All Items", Path.Combine(appdir, "SmallMarkerImages", "alchemy.png"), null);
            Markers = new ObservableCollection<MarkerToggleItemViewModel>
            {
                root
            };
            root.Children = new ObservableCollection<MarkerToggleItemViewModel>();
        }

        public void AddItemToRoot(string name, string pathToImage)
        {
            root.Children.Add(new MarkerToggleItemViewModel
                (
                    name,
                    Path.Combine(appdir, pathToImage),
                    root
                ));
        }

        public void Clear()
        {
            root.Children.Clear();
        }
    }
}