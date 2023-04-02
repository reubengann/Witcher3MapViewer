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
                var file = JsonSerializer.Deserialize<GwentStatusJsonFile>(File.ReadAllText("auto_gwent_statuses.json"));
                if (file == null) { throw new Exception("Could not find auto_gwent_statuses.json"); }
                Statuses = file.Statuses;
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
            string txt = JsonSerializer.Serialize(new GwentStatusJsonFile { Statuses = Statuses });
            File.WriteAllText(filename, txt);
        }
    }

    class GwentStatusJsonFile
    {
        public Dictionary<int, int> Statuses { get; set; }
    }
}
