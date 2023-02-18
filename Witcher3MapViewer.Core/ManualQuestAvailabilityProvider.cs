using System.Text.Json;

namespace Witcher3MapViewer.Core
{
    public abstract class ManualQuestAvailabilityProvider : IQuestAvailabilityProvider
    {
        protected Dictionary<string, QuestStatusState> _statuses = new Dictionary<string, QuestStatusState>();

        public event Action? AvailabilityChanged;

        public QuestStatusState GetState(string guid)
        {
            if (!_statuses.ContainsKey(guid)) return QuestStatusState.NotFound;
            return _statuses[guid];
        }

        public bool IsQuestAvailable(Quest q)
        {
            if (!q.HasAnyConditions) return true;
            foreach (var item in q.AvailableIfAny.Success)
            {
                if (_statuses.ContainsKey(item) && _statuses[item] >= QuestStatusState.Success)
                    return true;
            }
            return false;
        }

        public virtual void SetState(string guid, QuestStatusState? state)
        {
            if (state == null)
                _statuses.Remove(guid);
            else _statuses[guid] = (QuestStatusState)state;
        }
    }

    public class JsonManualQuestAvailabilityProvider : ManualQuestAvailabilityProvider
    {
        private readonly string filepath;

        public JsonManualQuestAvailabilityProvider(string filepath)
        {
            this.filepath = filepath;
            if (File.Exists(filepath))
            {
                var jsonString = File.ReadAllText(filepath);
                var read = JsonSerializer.Deserialize<Dictionary<string, QuestStatusState>>(jsonString);
                if (read != null)
                {
                    _statuses = read;
                }
            }
        }

        public override void SetState(string guid, QuestStatusState? state)
        {
            base.SetState(guid, state);
            var write = JsonSerializer.Serialize(_statuses);
            File.WriteAllText(filepath, write);
        }
    }
}