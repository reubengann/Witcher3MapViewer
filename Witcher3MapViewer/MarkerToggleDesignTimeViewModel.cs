using System.Collections.Generic;
using System.IO;
using Witcher3MapViewer.Core;

namespace Witcher3MapViewer.WPF
{
    public class MarkerToggleDesignTimeViewModel : BaseViewModel
    {
        string appdir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public List<MarkerToggleItemDesignTimeViewModel> Markers { get; set; }
        public MarkerToggleDesignTimeViewModel()
        {
            Markers = new List<MarkerToggleItemDesignTimeViewModel>
            {
                new MarkerToggleItemDesignTimeViewModel("All Items", Path.Combine(appdir, "SmallMarkerImages", "alchemy.png"), null)
            };
            Markers[0].Children = new List<MarkerToggleItemDesignTimeViewModel>()
            {
                new MarkerToggleItemDesignTimeViewModel
                (
                    "Item 1",
                    Path.Combine(appdir, "SmallMarkerImages", "Armorer.png"),
                    Markers[0]
                ),
                new MarkerToggleItemDesignTimeViewModel
                (
                    "Item 2",
                    Path.Combine(appdir, "SmallMarkerImages", "BanditCampfire.png"),
                    Markers[0]
                )
            };
        }
    }
}
