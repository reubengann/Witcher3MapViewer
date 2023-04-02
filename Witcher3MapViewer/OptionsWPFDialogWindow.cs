using Witcher3MapViewer.Core;
using Witcher3MapViewer.Core.Interfaces;

namespace Witcher3MapViewer.WPF
{
    public class OptionsWPFDialogWindow : IOptionsDialogWindow
    {
        Options options;
        bool ResetRequested = false;
        bool ChangedTrackingMode = false;

        public OptionsWPFDialogWindow(Options options)
        {
            this.options = options;
        }

        public Options GetNewOptions()
        {
            return options;
        }

        public bool ModeChanged()
        {
            return ChangedTrackingMode;
        }

        public bool ResetWasRequested()
        {
            return ResetRequested;
        }

        public bool ShowDialog()
        {
            ResetRequested = false;
            OptionsWindow optionsWindow = new OptionsWindow();
            OptionsWindowViewModel optionsWindowViewModel = new OptionsWindowViewModel(options);
            optionsWindow.DataContext = optionsWindowViewModel;
            var result = optionsWindow.ShowDialog();
            if (result == true)
            {
                ResetRequested = optionsWindowViewModel.RequestedResetOfQuests;
                if (options.TrackingMode != optionsWindowViewModel.NewOptions.TrackingMode)
                {
                    ChangedTrackingMode = true;
                }
                options = optionsWindowViewModel.NewOptions;
            }
            return result ?? false;
        }
    }
}
