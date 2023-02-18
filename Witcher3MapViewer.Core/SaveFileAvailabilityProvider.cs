using SaveFile;

namespace Witcher3MapViewer.Core
{
    public class SaveFileAvailabilityProvider : IQuestAvailabilityProvider
    {
        FileSystemWatcher _fileSystemWatcher;
        private readonly string _saveFileDirectory;
        private readonly JsonManualQuestAvailabilityProvider _localProgress;
        private readonly string _tempfolder;
        private Witcher3SaveFile _saveFile;

        public event Action? AvailabilityChanged;

        public int PlayerLevel => _saveFile.CharacterLevel;

        public SaveFileAvailabilityProvider(string saveFileDirectory, string localProgressFilename, string tempfolder)
        {
            _saveFileDirectory = saveFileDirectory;
            _localProgress = new JsonManualQuestAvailabilityProvider(localProgressFilename);
            _tempfolder = tempfolder;
            _saveFile = LoadNewestFile();
            _fileSystemWatcher = new FileSystemWatcher(_saveFileDirectory);
            SetUpFilewatcher(_fileSystemWatcher);
        }

        private Witcher3SaveFile LoadNewestFile()
        {
            DirectoryInfo directory = new DirectoryInfo(_saveFileDirectory);
            FileInfo myFile = (from f in directory.GetFiles("*.sav")
                               orderby f.LastWriteTime descending
                               select f).First();
            bool success = CopyWhenAvailable(myFile.FullName);
            if (!success) { throw new Exception("File is locked even after 5 seconds"); }
            return new Witcher3SaveFile(Path.Combine(_tempfolder, myFile.Name), Witcher3ReadLevel.Quick);
        }

        public QuestStatusState GetState(string guid)
        {
            var localState = _localProgress.GetState(guid);
            if (localState != QuestStatusState.NotFound) return localState;
            if (!_saveFile.CJournalManager.StatusDict.ContainsKey(guid))
            {
                return QuestStatusState.NotFound;
            }
            Witcher3JournalEntryStatus status = _saveFile.CJournalManager.StatusDict[guid];
            return (QuestStatusState)status.Status;
        }

        public bool IsQuestAvailable(Quest q)
        {
            if (!q.HasAnyConditions) return true;
            foreach (var item in q.AvailableIfAny.Success)
            {

                if (GetState(item) >= QuestStatusState.Success)
                    return true;
            }
            return false;
        }

        public void SetState(string guid, QuestStatusState? state)
        {
            _localProgress.SetState(guid, state);
        }

        private void SetUpFilewatcher(FileSystemWatcher watcher)
        {
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.sav";
            watcher.Changed += _fileSystemWatcher_Changed;
            watcher.Created += _fileSystemWatcher_Changed;
            watcher.Renamed += _fileSystemWatcher_Changed;
            watcher.EnableRaisingEvents = true;
        }

        private void _fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(2000);
            _saveFile = LoadNewestFile();
            AvailabilityChanged?.Invoke();
        }

        private bool CopyWhenAvailable(string fullPath)
        {
            int numTries = 0;
            while (true)
            {
                numTries++;
                try
                {
                    File.Copy(fullPath, Path.Combine(_tempfolder, Path.GetFileName(fullPath)), true);
                    return true;
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
                catch (Exception) { return false; }
            }
        }
    }
}
