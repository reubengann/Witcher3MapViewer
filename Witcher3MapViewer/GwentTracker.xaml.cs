using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Witcher3MapViewer
{
    /// <summary>
    /// Interaction logic for GwentTracker.xaml
    /// </summary>
    public partial class GwentTracker : Window
    {

        public event PropertyChangedEventHandler PropertyChanged;
        List<Witcher3GwentCard> BaseGameCards;
        public event EventHandler RequestedGwentPlayersOnMap;
        private bool ShownOnMap = false;

        ObservableCollection<GwentCardViewModel> _cards;
        public ObservableCollection<GwentCardViewModel> Cards { get { return _cards; } }

        string _collectEmAllStatus = "Main game cards go here";
        public string CollectEmAllStatus { get { return _collectEmAllStatus; }
            set {
                _collectEmAllStatus = value;
                RaisePropertyChanged("CollectEmAllStatus");
            } }

        string _skelligeDeckStatus = "DLC cards go here";

        int numowned = 0;

        public GwentTracker()
        {
            _cards = new ObservableCollection<GwentCardViewModel>();
            InitializeComponent();
            DataContext = this;
        }

        public void LoadInfo(List<Witcher3GwentCard> _baseGameCards,
            List<Witcher3GwentCard> OwnedUnitCards, List<Witcher3GwentCard> OwnedLeaderCards)
        {
            BaseGameCards = _baseGameCards;
            ObservableCollection<GwentCardViewModel> temp = new ObservableCollection<GwentCardViewModel>();
            Dictionary<int, Witcher3GwentCard> OwnedCards = new Dictionary<int, Witcher3GwentCard>();
            foreach (Witcher3GwentCard thecard in OwnedUnitCards)
                OwnedCards[thecard.cardIndex] = thecard;
            foreach (Witcher3GwentCard thecard in OwnedLeaderCards)
                OwnedCards[thecard.cardIndex] = thecard;

            numowned = 0;
            foreach (Witcher3GwentCard thecard in BaseGameCards)
            {
                bool isowned = false;
                if (OwnedCards.ContainsKey(thecard.cardIndex))
                {
                    Witcher3GwentCard owned = OwnedCards[thecard.cardIndex];
                    thecard.numCopies = owned.numCopies;
                    isowned = true;
                    numowned++;
                }
                GwentCardViewModel model = new GwentCardViewModel(thecard);
                model.IsChecked = isowned;
                temp.Add(model);
            }
            temp.Sort();
            CollectEmAllStatus = "Collect 'em all: " + numowned.ToString() + "/" + BaseGameCards.Count.ToString();
            _cards = temp;
        }

        public void LoadInfo(List<Witcher3GwentCard> _baseGameCards)
        {
            BaseGameCards = _baseGameCards;
            ObservableCollection<GwentCardViewModel> temp = new ObservableCollection<GwentCardViewModel>();            

            numowned = 0;
            foreach (Witcher3GwentCard thecard in BaseGameCards)
            {                
                GwentCardViewModel model = new GwentCardViewModel(thecard);
                model.IsChecked = false;
                temp.Add(model);
            }
            temp.Sort();
            CollectEmAllStatus = "Collect 'em all: " + numowned.ToString() + "/" + BaseGameCards.Count.ToString();
            _cards = temp;
        }

        public void UpdateCards(List<Witcher3GwentCard> OwnedUnitCards, List<Witcher3GwentCard> OwnedLeaderCards)
        {
            Dictionary<int, Witcher3GwentCard> OwnedCards = new Dictionary<int, Witcher3GwentCard>();
            foreach (Witcher3GwentCard thecard in OwnedUnitCards)
                OwnedCards[thecard.cardIndex] = thecard;
            foreach (Witcher3GwentCard thecard in OwnedLeaderCards)
                OwnedCards[thecard.cardIndex] = thecard;

            numowned = 0;
            foreach(GwentCardViewModel model in _cards)
            {
                if (OwnedCards.ContainsKey(model.ID))
                {
                    Witcher3GwentCard owned = OwnedCards[model.ID];
                    model.IsChecked = true;
                    model.NumHeld = owned.numCopies;
                    numowned++;
                }
            }
            CollectEmAllStatus = "Collect 'em all: " + numowned.ToString() + "/" + BaseGameCards.Count.ToString();
        }

        private void RaisePropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ShowOnMapButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ShownOnMap)            
                ShowOnMapButton.Content = "Hide players on map";                
            else ShowOnMapButton.Content = "Show players on map";
            RequestedGwentPlayersOnMap(this, null);
        }

        
    }
}
