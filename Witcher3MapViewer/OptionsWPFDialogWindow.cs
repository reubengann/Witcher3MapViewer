using Witcher3MapViewer.Core;
using Witcher3MapViewer.Core.Interfaces;

namespace Witcher3MapViewer.WPF
{
    public class OptionsWPFDialogWindow : IOptionsDialogWindow
    {
        Options options;
        bool ResetRequested = false;

        public OptionsWPFDialogWindow(Options options)
        {
            this.options = options;
        }

        public Options GetNewOptions()
        {
            return options;
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
            ResetRequested = optionsWindowViewModel.RequestedResetOfQuests;
            return result ?? false;
        }
    }
}
