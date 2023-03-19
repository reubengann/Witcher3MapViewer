using System.Collections.Generic;
using Witcher3MapViewer.Core;
using Witcher3MapViewer.Core.Interfaces;

namespace Witcher3MapViewer.WPF
{
    class GwentTrackerWPFWindow : IGwentTrackerWindow
    {
        public void LaunchWindow(List<GwentCard> BaseGameCards,
            IGwentStatusProvider gwentStatusProvider)
        {
            GwentTracker gt = new GwentTracker();
            gt.DataContext = new GwentTrackerViewModel(BaseGameCards, gwentStatusProvider);
            gt.Show();
        }
    }
}
