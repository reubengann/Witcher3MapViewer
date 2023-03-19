namespace Witcher3MapViewer.Core.Interfaces
{
    public interface IGwentTrackerWindow
    {
        void LaunchWindow(List<GwentCard> BaseGameCards,
            IGwentStatusProvider gwentStatusProvider);
    }
}
