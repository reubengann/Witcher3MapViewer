﻿using System.Collections.ObjectModel;

namespace Witcher3MapViewer.Core
{
    public class QuestListViewModel : BaseViewModel
    {
        public event Action<QuestViewModel>? ItemSelectedChanged;

        ObservableCollection<QuestViewModel> _currentQuests;
        private readonly ILevelProvider _levelProvider;

        public ObservableCollection<QuestViewModel> CurrentQuests { get { return _currentQuests; } }

        public QuestListViewModel(List<Quest> currentQuests, IQuestAvailabilityProvider questAvailabilityProvider, ILevelProvider levelProvider)
        {
            _currentQuests = new ObservableCollection<QuestViewModel>(
                currentQuests.Select(q => new QuestViewModel(q, questAvailabilityProvider, levelProvider, null))
                );
            foreach (var qvm in _currentQuests)
            {
                qvm.ItemWasChanged += RefreshAndSelectNew;
                qvm.SelectedWasChanged += ChildSelectedChanged;
            }
            questAvailabilityProvider.AvailabilityChanged += RefreshAndSelectNew;
            _levelProvider = levelProvider;
        }

        private void ChildSelectedChanged(QuestViewModel obj)
        {
            ItemSelectedChanged?.Invoke(obj);
        }

        private void RefreshAndSelectNew()
        {
            RefreshVisible();
            SelectBest();
        }

        private void RefreshVisible()
        {
            foreach (var qvm in _currentQuests)
                qvm.RefreshVisibility();

        }

        public void SelectBest()
        {
            int level = _levelProvider.GetLevel();
            QuestViewModel? best = null;
            foreach (QuestViewModel q in CurrentQuests)
            {
                if (q.IsChecked != true)
                {
                    if (q.QuestType != QuestType.Main && q.SuggestedLevel <= level + 2 && q.Visible == true)
                    {
                        q.IsSelected = true;
                        return;
                    }
                    else if (best == null)
                        best = q;
                }
            }
            if (best != null) best.IsSelected = true;
        }
    }

    public class QuestViewModel : BaseViewModel//, IComparable
    {
        public List<QuestViewModel> Children { get; private set; }
        public QuestViewModel? _parent;
        public Quest _quest;
        private readonly IQuestAvailabilityProvider _questAvailabilityProvider;
        public event Action? ItemWasChanged;
        public event Action<QuestViewModel>? SelectedWasChanged;

        public string Name => _quest.Name;
        public int SuggestedLevel => _quest.LevelRequirement;
        public int PlayerLevel => 0;
        public QuestType QuestType => _quest.QuestType;

        bool? _isSelected = false;

        public bool Visible
        {
            get
            {
                if (_parent != null)
                    return _questAvailabilityProvider.IsQuestAvailable(_parent._quest);
                return _questAvailabilityProvider.IsQuestAvailable(_quest);
            }
        }


        public QuestViewModel(Quest quest, IQuestAvailabilityProvider questAvailabilityProvider, ILevelProvider levelProvider, QuestViewModel? parent)
        {
            _parent = parent;
            _quest = quest;
            _questAvailabilityProvider = questAvailabilityProvider;
            Children = new List<QuestViewModel>();
            if (_quest.Objectives != null)
            {
                foreach (Quest o in _quest.Objectives)
                {
                    Children.Add(new QuestViewModel(o, questAvailabilityProvider, levelProvider, this));
                }
            }
            foreach (Quest sq in _quest.Subquests)
            {
                Children.Add(new QuestViewModel(sq, questAvailabilityProvider, levelProvider, this));
            }
        }


        public bool? IsChecked
        {
            get
            {
                QuestStatusState questStatusState = _questAvailabilityProvider.GetState(_quest.GUID);

                return questStatusState == QuestStatusState.Success;
            }
            set
            {
                if (value == true)
                    _questAvailabilityProvider.SetState(_quest.GUID, QuestStatusState.Success);
                else
                    _questAvailabilityProvider.SetState(_quest.GUID, null);
                SetIsChecked(value, true, false);
                ItemWasChanged?.Invoke();
            }
        }

        public bool? IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                if (value == true)
                    SelectedWasChanged?.Invoke(this);
            }
        }

        public void RefreshVisibility()
        {
            OnPropertyChanged(nameof(Visible));
            OnPropertyChanged(nameof(IsChecked));
            foreach (var c in Children)
            {
                c.RefreshVisibility();
            }
        }

        //private bool? _isDeferred = false;
        //public bool? IsDeferred
        //{
        //    get { return _isDeferred; }
        //    set
        //    {
        //        _isDeferred = value;
        //        if (_isDeferred != null)
        //        {
        //            _quest.Deferred = (bool)_isDeferred;
        //            OnPropertyChanged("Name");
        //        }
        //    }
        //}


        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (updateChildren && value.HasValue)
                Children.ForEach(c => c.SetIsChecked(value, true, false));

            //if (updateParent && _parent != null)
            //    _parent.VerifyCheckState();

            OnPropertyChanged(nameof(IsChecked));
        }

        //void VerifyCheckState()
        //{
        //    bool? state = null;
        //    for (int i = 0; i < Children.Count; ++i)
        //    {
        //        bool? current = Children[i].IsChecked;
        //        if (i == 0)
        //        {
        //            state = current;
        //        }
        //        else if (state != current)
        //        {
        //            state = null;
        //            break;
        //        }
        //    }
        //    SetIsChecked(state, false, true);
        //}

        //public void Recheck()
        //{
        //    //if(IsChecked == false && correspondingQuest.Status > QuestStatusState.Active)

        //    //if (IsChecked == true && !correspondingQuest.Done)
        //    //    IsChecked = false;
        //    //else if (IsChecked == false && correspondingQuest.Done)
        //    //    IsChecked = true;
        //    IsChecked = _quest.Done;
        //    _quest.Forced = false;
        //    foreach (QuestViewModel child in Children)
        //        child.Recheck();
        //}


        //public static ObservableCollection<QuestViewModel> CreateChooser(Witcher3ProgressStatus progressStatus, bool availableonly)
        //{
        //    if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
        //        return new ObservableCollection<QuestViewModel>();

        //    ObservableCollection<QuestViewModel> items = new ObservableCollection<QuestViewModel>();

        //    List<Quest> TheList;
        //    if (availableonly)
        //        TheList = progressStatus.AvailableQuests;
        //    else TheList = progressStatus.Quests;

        //    foreach (Quest q in TheList)
        //    {
        //        QuestViewModel root = new QuestViewModel(q);
        //        items.Add(root);
        //    }


        //    return items;
        //}

        //public int CompareTo(object o)
        //{
        //    QuestViewModel a = this;
        //    QuestViewModel b = (QuestViewModel)o;
        //    int ret = a._quest.Done.CompareTo(b._quest.Done);
        //    if (ret != 0) return -ret;
        //    ret = a._quest.Deferred.CompareTo(b._quest.Deferred);
        //    if (ret != 0) return -ret;
        //    ret = a._quest.LevelRequirement.CompareTo(b._quest.LevelRequirement);
        //    if (ret != 0) return ret;
        //    return a._quest.UniqueID.CompareTo(b._quest.UniqueID);

        //}
    }
}
