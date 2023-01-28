using SaveFile;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Witcher3MapViewer.Core;

namespace Witcher3MapViewer.WPF
{
    public class SaveFileAvailabilityProvider : IQuestAvailabilityProvider
    {
        FileSystemWatcher _fileSystemWatcher;
        private readonly string _saveFileDirectory;
        private readonly string _localProgressFilename;
        private readonly string _tempfolder;

        public event Action? AvailabilityChanged;

        public SaveFileAvailabilityProvider(string saveFileDirectory, string localProgressFilename, string tempfolder)
        {
            _saveFileDirectory = saveFileDirectory;
            _localProgressFilename = localProgressFilename;
            _tempfolder = tempfolder;
            LoadNewestFile();
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
            throw new NotImplementedException();
        }

        public bool IsQuestAvailable(Quest q)
        {
            throw new NotImplementedException();
        }

        public void SetState(string guid, QuestStatusState? state)
        {
            throw new NotImplementedException();
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
            LoadNewestFile();
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
