namespace Witcher3MapViewer.Core
{
    public class SaveFileLevelProvider : ILevelProvider
    {
        SaveFileAvailabilityProvider _save;

        public SaveFileLevelProvider(SaveFileAvailabilityProvider save)
        {
            _save = save;
        }

        public event Action LevelChanged;

        public int GetLevel()
        {
            return _save.PlayerLevel;
        }

        public void SetLevel(int level)
        {
        }
    }
}
