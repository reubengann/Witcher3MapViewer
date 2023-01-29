namespace Witcher3MapViewer.Core
{
    public class ManualQuestAvailabilityProvider : IQuestAvailabilityProvider
    {
        private Dictionary<string, QuestStatusState> _statuses = new Dictionary<string, QuestStatusState>();

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

        public void SetState(string guid, QuestStatusState? state)
        {
            if (state == null)
                _statuses.Remove(guid);
            else _statuses[guid] = (QuestStatusState)state;
        }
    }
}