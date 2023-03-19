namespace Witcher3MapViewer.Core
{
    public interface IGwentStatusProvider
    {
        event Action? StatusUpdated;
        int GetCount(int id);
    }
}