using Witcher3MapViewer.Core.Interfaces;

namespace Witcher3MapViewer.WPF
{
    public class MainWPFWindow : IMainWindow
    {
        private readonly MainWindow mw;

        public MainWPFWindow(MainWindow mw)
        {
            this.mw = mw;
        }

        public void Close()
        {
            mw.Close();
        }
    }
}
