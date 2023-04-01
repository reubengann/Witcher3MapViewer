using Witcher3MapViewer.Core;
using Witcher3MapViewer.Core.Interfaces;

namespace Witcher3MapViewer.WPF
{
    public class OptionsWPFDialogWindow : IOptionsDialogWindow
    {
        Options options;

        public OptionsWPFDialogWindow(Options options)
        {
            this.options = options;
        }

        public Options GetNewOptions()
        {
            return options;
        }

        public bool ShowDialog()
        {
            OptionsWindow optionsWindow = new OptionsWindow();
            OptionsWindowViewModel optionsWindowViewModel = new OptionsWindowViewModel(options);
            optionsWindow.DataContext = optionsWindowViewModel;
            var result = optionsWindow.ShowDialog();
            return result ?? false;
        }
    }
}
