namespace Witcher3MapViewer.Core
{
    public class SaveFileLevelProvider : ILevelProvider
    {
        SaveFileAvailabilityProvider _save;

        public SaveFileLevelProvider(SaveFileAvailabilityProvider save)
        {
            _save = save;
            _save.AvailabilityChanged += _save_AvailabilityChanged;
        }

        private void _save_AvailabilityChanged()
        {
            LevelChanged?.Invoke();
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
