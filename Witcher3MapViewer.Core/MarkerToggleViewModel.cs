using System.Collections.ObjectModel;

namespace Witcher3MapViewer.Core
{
    public class MarkerToggleViewModel : BaseViewModel
    {
        string appdir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        MarkerToggleItemViewModel root;
        private readonly IMap _map;

        public ObservableCollection<MarkerToggleItemViewModel> Markers { get; set; }
        public MarkerToggleViewModel(IMap map)
        {
            root = new MarkerToggleItemViewModel("All Items", Path.Combine(appdir, "SmallMarkerImages", "alchemy.png"), null, _map, 0);
            Markers = new ObservableCollection<MarkerToggleItemViewModel>
            {
                root
            };
            root.Children = new ObservableCollection<MarkerToggleItemViewModel>();
            _map = map;
        }

        public void AddItemToRoot(string name, string pathToImage, int layerNum)
        {
            root.Children.Add(new MarkerToggleItemViewModel
                (
                    name,
                    Path.Combine(appdir, pathToImage),
                    root,
                    _map,
                    layerNum
                ));
        }

        public void Clear()
        {
            root.Children.Clear();
        }
    }
}