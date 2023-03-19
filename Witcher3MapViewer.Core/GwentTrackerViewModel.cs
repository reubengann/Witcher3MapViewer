using System.Collections.ObjectModel;

namespace Witcher3MapViewer.Core
{
    public class GwentTrackerViewModel : BaseViewModel
    {
        private ObservableCollection<GwentCardViewModel> _cards;

        public ObservableCollection<GwentCardViewModel> Cards
        {
            get { return _cards; }
            set { _cards = value; }
        }

        public string CollectEmAllStatus
        {
            get
            {
                int numOwned = _cards.Select(x => x.IsChecked ? 1 : 0).Sum();
                return $"Collect 'em all: {numOwned}/{_cards.Count}";
            }
        }


        public GwentTrackerViewModel(List<GwentCard> BaseGameCards,
            IGwentStatusProvider gwentStatusProvider)
        {
            _cards = new ObservableCollection<GwentCardViewModel>();
            foreach (var card in BaseGameCards)
            {
                _cards.Add(new GwentCardViewModel(card, gwentStatusProvider));
            }
            gwentStatusProvider.StatusUpdated += GwentStatusProvider_StatusUpdated;
        }

        private void GwentStatusProvider_StatusUpdated()
        {
            OnPropertyChanged(nameof(CollectEmAllStatus));
        }
    }

    public class GwentCardViewModel : BaseViewModel
    {
        private readonly GwentCard thecard;
        private readonly IGwentStatusProvider gwentStatusProvider;

        public GwentCardViewModel(GwentCard thecard, IGwentStatusProvider gwentStatusProvider)
        {
            this.thecard = thecard;
            this.gwentStatusProvider = gwentStatusProvider;
            gwentStatusProvider.StatusUpdated += GwentStatusProvider_StatusUpdated;
        }

        private void GwentStatusProvider_StatusUpdated()
        {
            OnPropertyChanged(nameof(IsChecked));
        }

        private bool _isChecked;

        public bool IsChecked
        {
            get { return gwentStatusProvider.GetCount(thecard.cardIndex) > 0; }
            set { _isChecked = value; }
        }


        public string Name => thecard.Name;
        public string Location => thecard.Location;
    }
}
