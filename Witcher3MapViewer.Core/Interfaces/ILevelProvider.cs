namespace Witcher3MapViewer.Core
{
    public interface ILevelProvider
    {
        event Action LevelChanged;
        int GetLevel();
        void SetLevel(int level);
    }
}
