namespace Witcher3MapViewer.Core
{
    public class SaveFileGwentStatusProvider : IGwentStatusProvider
    {
        private readonly SaveFileAvailabilityProvider saveFile;

        public SaveFileGwentStatusProvider(SaveFileAvailabilityProvider saveFile)
        {
            this.saveFile = saveFile;
        }

        public int GetCount(int id)
        {
            Dictionary<int, int> counts = saveFile.GetGwent();
            if (counts.ContainsKey(id))
            {
                return counts[id];
            }
            return 0;
        }
    }
}
