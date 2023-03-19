using Witcher3MapViewer.Core;
using Witcher3MapViewer.Core.Interfaces;

namespace Witcher3MapViewer.WPF
{
    public class OptionsWPFDialogWindow : IOptionsDialogWindow
    {
        public PolicyOptions? ShowDialog()
        {
            OptionsWindow optionsWindow = new OptionsWindow();
            OptionsWindowViewModel optionsWindowViewModel = new OptionsWindowViewModel();
            optionsWindow.DataContext = optionsWindowViewModel;
            var result = optionsWindow.ShowDialog();
            if (result != true)
            {
                return null;
            }
            var po = new PolicyOptions();
            po.ShowRaces = optionsWindowViewModel.ShowRaces;
            return po;

        }
    }
}
