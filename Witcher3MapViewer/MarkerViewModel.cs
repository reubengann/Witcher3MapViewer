using System.Collections.Generic;
using System.ComponentModel;

namespace Witcher3MapViewer
{
    public class MarkerViewModel
    {
        bool? _isChecked = false;
        public List<MarkerViewModel> Children { get; private set; }
        MarkerViewModel _parent;
        public event PropertyChangedEventHandler PropertyChanged;
        public string Name { get; private set; }
        private string _text;
        public string Text
        {
            get { return _text; }
            set { _text = value; OnPropertyChanged("Text"); }
        }

        private Mapsui.Layers.MemoryLayer _layer;
        public Mapsui.Layers.MemoryLayer AssociatedLayer
        {
            get { return _layer; }
            //set { _layer = value; }
        }

        public MarkerViewModel(string name, string text)
        {
            Name = name;
            Text = text;
            Children = new List<MarkerViewModel>();
        }

        public MarkerViewModel(MapPinType pintype, Mapsui.Layers.MemoryLayer layer)
        {
            Name = pintype.InternalName;
            Text = pintype.Name;
            _layer = layer;
            Children = new List<MarkerViewModel>();
            _isChecked = _layer.Enabled;
            _smallIconName = pintype.IconFile;
        }

        public bool? IsChecked
        {
            get { return _isChecked; }
            set { SetIsChecked(value, true, true); }
        }

        private string _smallIconName;
        private string _smallIconFolder;
        public string SmallIconPath { get
            {
                if (_smallIconName != null)
                    return System.IO.Path.Combine(_smallIconFolder, _smallIconName);
                else return null;
            }
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;
            if(_layer != null)
                _layer.Enabled = value ?? false;

            if (updateChildren && _isChecked.HasValue)
                Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

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
                    state = null;
                    break;
                }
            }
            SetIsChecked(state, false, true);
        }

        void OnPropertyChanged(string prop)
        {            
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));            
        }
        

        //public static List<MarkerViewModel> CreateChooser(List<MapPinType> Pintypes)
        //{
        //    if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
        //        return new List<MarkerViewModel>();

        //    MarkerViewModel root = new MarkerViewModel("All", "All Items");
        //    root.IsChecked = true;
        //    foreach(MapPinType mpt in Pintypes)
        //    {
        //        MarkerViewModel child = new MarkerViewModel(mpt.InternalName, mpt.Name);
        //        child.IsChecked = mpt.Shown;
        //        root.Children.Add(child);
        //    }
        //    root.Initialize();
        //    return new List<MarkerViewModel> { root };
        //}

        public static MarkerViewModel CreateRoot(string iconfolder)
        {
            MarkerViewModel root = new MarkerViewModel("All", "All Items");
            root._smallIconFolder = iconfolder;
            root.IsChecked = true;
            return root;
        }

        public MarkerViewModel FindChild(string query)
        {
            foreach(MarkerViewModel child in Children)
            {
                if (child.Name == query)
                    return child;
            }
            return null;
        }

        void Initialize()
        {
            foreach (MarkerViewModel child in Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }

        public void AddChild(MarkerViewModel child)
        {
            child._parent = this;
            child._smallIconFolder = _smallIconFolder;
            Children.Add(child);            
        }
    }
}
