namespace Witcher3MapViewer.Core
{
    public class SaveFileGwentStatusProvider : IGwentStatusProvider
    {
        private readonly SaveFileAvailabilityProvider saveFile;

        public SaveFileGwentStatusProvider(SaveFileAvailabilityProvider saveFile)
        {
            this.saveFile = saveFile;
            this.saveFile.AvailabilityChanged += SaveFile_AvailabilityChanged;
        }

        private void SaveFile_AvailabilityChanged()
        {
            StatusUpdated?.Invoke();
        }

        public event Action? StatusUpdated;

        public int GetCount(int id)
        {
            Dictionary<int, int> counts = saveFile.GetGwent();
            if (counts.ContainsKey(id))
            {
                return counts[id];
            }
            return 0;
        }

        public void SetCount(int id, int count)
        {

        }
    }
}
