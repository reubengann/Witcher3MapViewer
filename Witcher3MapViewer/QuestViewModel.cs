using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Witcher3MapViewer
{
    public class QuestViewModel : INotifyPropertyChanged, IComparable
    {
        bool? _isChecked = false;
        public List<QuestViewModel> Children { get; private set; }
        public QuestViewModel _parent;
        public Quest correspondingQuest;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public string Name
        {
            get
            {
                if (IsDeferred == true)
                    return correspondingQuest.Name + " (D)";
                else return correspondingQuest.Name;
            }
            //set
            //{
                
            //}
        }
        private int _id;
        public int SuggestedLevel { get; private set; }
        bool? _isSelected = false;
        public string id
        {
            get { return _id.ToString(); }
            set { _id = int.Parse(value); OnPropertyChanged("ID"); }
        }

        public QuestViewModel(Quest CorrespondingQuest)
        {
            correspondingQuest = CorrespondingQuest;
            //Name = correspondingQuest.Name;
            _id = correspondingQuest.UniqueID;
            _isChecked = correspondingQuest.Done;
            SuggestedLevel = correspondingQuest.LevelRequirement;
            Children = new List<QuestViewModel>();
            if (correspondingQuest.Subquests.Count != 0)
            {
                foreach (Quest sq in correspondingQuest.Subquests)
                {
                    QuestViewModel temp = new QuestViewModel(sq);
                    temp._parent = this;
                    Children.Add(temp);
                }
            }
        }

        public bool? IsChecked
        {
            get { return _isChecked; }
            set { SetIsChecked(value, true, false); }
        }

        public bool? IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        private bool? _isDeferred = false;
        public bool? IsDeferred
        {
            get { return _isDeferred; }
            set
            {
                _isDeferred = value;
                if (_isDeferred != null)
                {
                    correspondingQuest.Deferred = (bool)_isDeferred;
                    OnPropertyChanged("Name");
                }
            }
        }


        public void Touch()
        {
            RaisePropertyChanged("SuggestedLevel");
        }

        void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(prop));
        }

        private void RaisePropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;
            if (_isChecked == true)
            {
                correspondingQuest.Status = QuestStatusState.Success;
                //correspondingQuest.Forced = true;
            }
            else
            {
                correspondingQuest.Status = QuestStatusState.NotFound;
            }

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

        public void Recheck()
        {
            //if(IsChecked == false && correspondingQuest.Status > QuestStatusState.Active)

            //if (IsChecked == true && !correspondingQuest.Done)
            //    IsChecked = false;
            //else if (IsChecked == false && correspondingQuest.Done)
            //    IsChecked = true;
            IsChecked = correspondingQuest.Done;
            correspondingQuest.Forced = false;
            foreach (QuestViewModel child in Children)
                child.Recheck();
        }


        public static ObservableCollection<QuestViewModel> CreateChooser(Witcher3ProgressStatus progressStatus, bool availableonly)
        {
            if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
                return new ObservableCollection<QuestViewModel>();

            ObservableCollection<QuestViewModel> items = new ObservableCollection<QuestViewModel>();

            List<Quest> TheList;
            if (availableonly)
                TheList = progressStatus.AvailableQuests;
            else TheList = progressStatus.Quests;

            foreach (Quest q in TheList)
            {
                QuestViewModel root = new QuestViewModel(q);
                items.Add(root);
            }


            return items;
        }

        public int CompareTo(object o)
        {
            QuestViewModel a = this;
            QuestViewModel b = (QuestViewModel)o;
            int ret = a.correspondingQuest.Done.CompareTo(b.correspondingQuest.Done);
            if (ret != 0) return -ret;
            ret = a.correspondingQuest.Deferred.CompareTo(b.correspondingQuest.Deferred);
            if (ret != 0) return -ret;
            ret = a.correspondingQuest.LevelRequirement.CompareTo(b.correspondingQuest.LevelRequirement);
            if (ret != 0) return ret;
            return a.correspondingQuest.UniqueID.CompareTo(b.correspondingQuest.UniqueID);

        }
    }
}
