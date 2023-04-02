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
                GwentCardViewModel item = new GwentCardViewModel(card, gwentStatusProvider);
                item.StatusChanged += GwentStatusProvider_StatusUpdated;
                _cards.Add(item);
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
        public event Action StatusChanged;

        public GwentCardViewModel(GwentCard thecard, IGwentStatusProvider gwentStatusProvider)
        {
            this.thecard = thecard;
            this.gwentStatusProvider = gwentStatusProvider;
            gwentStatusProvider.StatusUpdated += GwentStatusProvider_StatusUpdated;
            _isChecked = gwentStatusProvider.GetCount(thecard.cardIndex) > 0;
        }

        private void GwentStatusProvider_StatusUpdated()
        {
            OnPropertyChanged(nameof(IsChecked));
        }

        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                gwentStatusProvider.SetCount(thecard.cardIndex, value == true ? 1 : 0);
                OnPropertyChanged(nameof(IsChecked));
                StatusChanged?.Invoke();
            }
        }


        public string Name => thecard.Name;
        public string Location => thecard.Location;
    }
}
