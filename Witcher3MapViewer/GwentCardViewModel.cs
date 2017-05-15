using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witcher3MapViewer
{
    public class GwentCardViewModel : INotifyPropertyChanged, IComparable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        bool _isChecked = false;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { _isChecked = value; }
        }

        int _id;
        public int ID { get { return _id; } }

        int _numHeld;
        public int NumHeld { get { return _numHeld; } set { _numHeld = value; OnPropertyChanged("NumHeld"); } }

        string _name;
        public string Name
        {
            set { _name = value; }
            get
            {
                if (_name == null || _name == "") return "Unknown";
                else return _name;
            }
        }

        string _location;
        public string Location { get { return _location; } }

        public GwentCardViewModel(Witcher3GwentCard thecard)
        {
            _id = thecard.cardIndex;
            _numHeld = thecard.numCopies;
            _name = thecard.Name;
            _location = thecard.Location;
        }       

        void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public int CompareTo(object o)
        {
            GwentCardViewModel a = this;
            GwentCardViewModel b = (GwentCardViewModel)o;
            //bool aChecked = a.IsChecked ?? false;
            //int ret = aChecked.CompareTo(b.IsChecked);
            //if (ret != 0) return -ret;
            return a.Name.CompareTo(b.Name);

        }
    }
}
