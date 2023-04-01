using Prism.Commands;
using System.Windows.Input;

namespace Witcher3MapViewer.Core
{
    public class OptionsWindowViewModel : BaseViewModel
    {
        readonly Options options;

        public OptionsWindowViewModel(Options options)
        {
            this.options = options;
            if (options.TrackingMode == TrackingMode.Automatic) CheckPathBox();
        }

        public bool ShowOnlyAvailable { get => options.ShowOnlyAvailable; set => options.ShowOnlyAvailable = value; }
        public bool ShowComplete { get => options.ShowComplete; set => options.ShowComplete = value; }
        public bool IsManualModeChecked
        {
            get => options.TrackingMode == TrackingMode.Manual;
            set
            {
                options.TrackingMode = TrackingMode.Manual;
                OnPropertyChanged(nameof(IsFileModeChecked));
            }
        }
        public bool IsFileModeChecked
        {
            get => options.TrackingMode == TrackingMode.Automatic;
            set
            {
                options.TrackingMode = TrackingMode.Automatic;
                CheckPathBox();
                OnPropertyChanged(nameof(IsManualModeChecked));
                OnPropertyChanged(nameof(IsFileModeChecked));
            }
        }

        private void CheckPathBox()
        {
            if (string.IsNullOrEmpty(SaveFilePath))
            {
                ErrorMessage = "Please enter a path";
                PathOk = false;
                return;
            }
            if (!Directory.Exists(SaveFilePath))
            {
                ErrorMessage = "Not a valid directory";
                PathOk = false;
                return;
            }
            ErrorMessage = "Path looks good";
            PathOk = true;
        }
        private bool pathOk;

        public bool PathOk
        {
            get { return pathOk; }
            set { pathOk = value; OnPropertyChanged(nameof(PathOk)); }
        }


        public string SaveFilePath
        {
            get => options.SaveFilePath;
            set
            {
                options.SaveFilePath = value;
                CheckPathBox();
            }
        }
        public ICommand FindSaveFilePathCommand => new DelegateCommand(FindPath);
        private string errorMessage = "";

        public string ErrorMessage
        {
            get { return errorMessage; }
            set { errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }


        private void FindPath()
        {
            string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string ersatzpath = Path.Combine(myDocuments, "The Witcher 3", "gamesaves");
            if (Directory.Exists(ersatzpath))
            {
                SaveFilePath = ersatzpath;
                OnPropertyChanged(nameof(SaveFilePath));
            }
            else
            {
                ErrorMessage = "Sorry, couldn't find the save path";
            }
        }
    }
}
