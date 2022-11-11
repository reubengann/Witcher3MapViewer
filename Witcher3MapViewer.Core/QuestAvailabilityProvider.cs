namespace Witcher3MapViewer.Core
{
    public class QuestAvailabilityProvider : IQuestAvailabilityProvider
    {
        private Dictionary<string, QuestStatusState> _statuses = new Dictionary<string, QuestStatusState>();

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

        public void SetState(string guid, QuestStatusState state)
        {
            _statuses[guid] = state;
        }
    }
}