namespace Witcher3MapViewer.Core
{
    public interface IGwentStatusProvider
    {
        event Action? StatusUpdated;
        int GetCount(int id);
        void SetCount(int id, int count);
    }
}