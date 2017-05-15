using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Witcher3MapViewer;

namespace GwentDiscovery
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string SaveFolder = @"C:\Users\Reuben\Documents\The Witcher 3\gamesaves";
        private string appdir;
        Witcher3SaveFile ActiveSave;
        string ActiveSaveFileCopy;
        FileSystemWatcher _fileSystemWatcher;
        bool SaveChanged = false;
        ManualResetEvent _refreshRequested = new ManualResetEvent(false);
        List<Witcher3GwentCard> BaseGame;
        List<Witcher3GwentCard> BloodAndWine;
        ObservableCollection<GwentCardViewModel> _cards;
        public ObservableCollection<GwentCardViewModel> Cards { get { return _cards; } }
        System.Diagnostics.Stopwatch updatedtimer = new System.Diagnostics.Stopwatch();

        string _statusText = "This is the status";
        public string StatusText { get
            {
                return _statusText;
            }
            set
            {
                _statusText = value;
            }
        }
        int numowned = 0;

        public MainWindow()
        {
            DataContext = this;
            _cards = new ObservableCollection<GwentCardViewModel>();
            appdir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            GetGwentCardList();
            GetMostRecentSaveFile();
            SetGwentCardStatuses();
            _cards.Sort();
            InitializeComponent();




            SetUpFilewatcher();
        }

        private void SetGwentCardStatuses()
        {
            Dictionary<int, Witcher3GwentCard> OwnedCards = new Dictionary<int, Witcher3GwentCard>();
            foreach (Witcher3GwentCard thecard in ActiveSave.GwentManager.RegularCards)
                OwnedCards[thecard.cardIndex] = thecard;
            foreach (Witcher3GwentCard thecard in ActiveSave.GwentManager.LeaderCards)
                OwnedCards[thecard.cardIndex] = thecard;

            numowned = 0;
            foreach (Witcher3GwentCard thecard in BaseGame)
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
                _cards.Add(model);
            }
            StatusText = "Currently have [" + numowned.ToString() + "/" + BaseGame.Count.ToString() + "]";
            //foreach (Witcher3GwentCard thecard in ActiveSave.GwentManager.LeaderCards)
            //    _cards.Add(new GwentCardViewModel(thecard));
        }

        private void GetGwentCardList()
        {
            var reader = new Witcher3MapViewer.Readers.GwentXMLReader(Path.Combine(@"C:\Users\Reuben\Dropbox\Programs\Witcher3MapViewer\Witcher3MapViewer\Witcher3MapViewer\bin\x64\Debug", "Gwent.xml"));

            BaseGame = new List<Witcher3GwentCard>();
            foreach (Witcher3MapViewer.Readers.GwentCardAsRead cardasread in reader.Sets[0].Cards)
            {
                Witcher3GwentCard card = new Witcher3GwentCard();
                card.cardIndex = cardasread.ID;
                card.Name = cardasread.Name;
                card.Location = cardasread.Location;
                card.AssociatedQuest = cardasread.AssociatedQuest;
                BaseGame.Add(card);
            }
        }

        private void GetMostRecentSaveFile()
        {
            //Get file listing, find most recent file
            DirectoryInfo directory = new DirectoryInfo(SaveFolder);
            FileInfo myFile = (from f in directory.GetFiles("*.sav")
                               orderby f.LastWriteTime descending
                               select f).First();
            //copy to local directory
            if (CopyWhenAvailable(myFile.FullName))
            {
                myFile = new FileInfo(Path.Combine(appdir, "SavedGames", myFile.Name));
                ActiveSaveFileCopy = myFile.FullName;
                ActiveSave = new Witcher3SaveFile(myFile.FullName, Witcher3ReadLevel.Quick);
            }
        }

        private bool CopyWhenAvailable(string fullPath)
        {
            int numTries = 0;
            while (true)
            {
                numTries++;
                try
                {
                    File.Copy(fullPath, Path.Combine(appdir, "SavedGames", Path.GetFileName(fullPath)), true);
                    break;
                }
                catch (System.Reflection.AmbiguousMatchException) //this is just so we don't assign the exception to a variable
                {
                    //Failed to get access
                    if (numTries > 10)
                    {
                        //give up
                        return false;
                    }
                    // Wait for the lock to be released
                    Thread.Sleep(500);
                }
            }
            //Success
            return true;
        }

        private void SetUpFilewatcher()
        {
            _fileSystemWatcher = new FileSystemWatcher(SaveFolder);
            _fileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            _fileSystemWatcher.Filter = "*.sav";
            _fileSystemWatcher.Changed += _fileSystemWatcher_Changed;
            _fileSystemWatcher.Created += _fileSystemWatcher_Changed;
            _fileSystemWatcher.Renamed += _fileSystemWatcher_Changed;
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void _fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(2000);
            SaveChanged = true;
            _refreshRequested.Set();
            ThreadPool.QueueUserWorkItem(RefreshIfNecessary);
        }

        private void RefreshIfNecessary(object state)
        {
            lock (this)
            {
                if (!_refreshRequested.WaitOne(0))
                {
                    // Refresh not necessary
                    return;
                }
                if (SaveChanged)
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        Dispatcher.BeginInvoke(new Action(ExecuteSaveChange));
                    });
                }
                SaveChanged = false;
                _refreshRequested.Reset();
            }
        }

        private void ExecuteSaveChange()
        {
            ClearLocalSave();
            GetMostRecentSaveFile();
            updatedtimer.Stop();
            //foreach (Witcher3GwentCard thecard in ActiveSave.GwentManager.RegularCards)
            //{
            //    if (!AlreadyHaveThatCard(thecard))
            //    {
            //        GwentCardViewModel gcvm = new GwentCardViewModel(thecard);
            //        gcvm.IsObtained = true;
            //        _cards.Add(gcvm);
            //    }
            //}
            //foreach (Witcher3GwentCard thecard in ActiveSave.GwentManager.LeaderCards)
            //{
            //    if (!AlreadyHaveThatCard(thecard))
            //    {
            //        GwentCardViewModel gcvm = new GwentCardViewModel(thecard);
            //        gcvm.IsObtained = true;
            //        _cards.Add(gcvm);
            //    }
            //}
            _cards.Sort();
            updatedtimer.Reset();
            updatedtimer.Start();
        }

        private bool AlreadyHaveThatCard(Witcher3GwentCard thecard)
        {
            //var foo = _cards.Where(item => item.IDNumber == thecard.cardIndex).FirstOrDefault();
            //if (foo != null)
            //{
            //    if (foo.NumHeld != thecard.numCopies)
            //    {
            //        foo.NumHeld = thecard.numCopies;
            //        foo.IsObtained = true;
            //    }
            //    else if(updatedtimer.ElapsedMilliseconds > 10000) foo.IsObtained = false;
            //    return true;
            //}
            //return false;
            return false;
        }

        private void ClearLocalSave()
        {
            if (File.Exists(ActiveSaveFileCopy))
                File.Delete(ActiveSaveFileCopy);
        }
    }

    static class Extensions
    {
        public static void Sort<T>(this ObservableCollection<T> collection) where T : IComparable
        {
            List<T> sorted = collection.OrderBy(x => x).ToList();
            for (int i = 0; i < sorted.Count(); i++)
                collection.Move(collection.IndexOf(sorted[i]), i);
        }
    }
}
