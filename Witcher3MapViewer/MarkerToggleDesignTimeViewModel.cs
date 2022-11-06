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

    public class MarkerToggleItemDesignTimeViewModel : BaseViewModel
    {
        public string Text { get; set; }
        public List<MarkerToggleItemDesignTimeViewModel> Children { get; set; }

        private bool? _isChecked = true;
        private readonly MarkerToggleItemDesignTimeViewModel? _parent;

        public bool? IsChecked
        {
            get { return _isChecked; }
            set { SetIsChecked(value, true, true); }
        }

        public string SmallIconPath { get; set; }
        public MarkerToggleItemDesignTimeViewModel(string text, string smallIconPath, MarkerToggleItemDesignTimeViewModel? parent)
        {
            Text = text;
            Children = new List<MarkerToggleItemDesignTimeViewModel>();
            SmallIconPath = smallIconPath;
            _parent = parent;
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            //cascade this to all children
            if (updateChildren && _isChecked.HasValue)
                Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

            //ask the parent to check its state and all its parents
            if (updateParent && _parent != null)
                _parent.VerifyCheckState();

            OnPropertyChanged("IsChecked");
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < Children.Count; ++i)
            {
                bool? current = Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    //if any item doesn't equal the first one, set the status to partial
                    state = null;
                    break;
                }
            }
            // update our state and all its parents
            SetIsChecked(state, false, true);
        }
    }
}
