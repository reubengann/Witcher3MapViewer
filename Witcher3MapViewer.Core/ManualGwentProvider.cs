using System.Text.Json;

namespace Witcher3MapViewer.Core
{
    public class ManualGwentProvider : IGwentStatusProvider
    {
        public event Action? StatusUpdated;
        private Dictionary<int, int> Statuses;
        private readonly string filename;

        public ManualGwentProvider(string filename)
        {
            if (File.Exists(filename))
            {
                var file = JsonSerializer.Deserialize<GwentStatusJsonFile>(File.ReadAllText(filename));
                if (file == null) { throw new Exception("Gwent status file broken"); }
                Statuses = file.Statuses;
            }
            else
            {
                Statuses = new Dictionary<int, int>();
                File.WriteAllText(filename, JsonSerializer.Serialize(new GwentStatusJsonFile { Statuses = Statuses }));
            }

            this.filename = filename;
        }

        public int GetCount(int id)
        {
            if (Statuses.ContainsKey(id))
                return Statuses[id];
            return 0;
        }

        public void SetCount(int id, int count)
        {
            Statuses[id] = count;
            File.WriteAllText(filename, JsonSerializer.Serialize(new GwentStatusJsonFile { Statuses = Statuses }));
        }
    }

    class GwentStatusJsonFile
    {
        public Dictionary<int, int> Statuses { get; set; }
    }
}
