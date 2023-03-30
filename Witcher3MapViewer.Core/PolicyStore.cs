namespace Witcher3MapViewer.Core
{
    public class PolicyStore
    {
        private readonly IQuestAvailabilityProvider _availabilityProvider;

        private bool ShowOnlyAvailable;
        private bool ShowComplete;

        public PolicyStore(IQuestAvailabilityProvider availabilityProvider)
        {
            _availabilityProvider = availabilityProvider;
            ShowOnlyAvailable = true;
            ShowComplete = false;
        }

        public bool CurrentPolicy(Quest q)
        {
            if (ShowOnlyAvailable && !_availabilityProvider.IsQuestAvailable(q))
            {
                return false;
            }
            if (!ShowComplete && _availabilityProvider.GetState(q.GUID) == QuestStatusState.Success)
            {
                return false;
            }
            if (q.HideIfAny != null && q.HideIfAny.AllConditions.Count > 0)
            {
                foreach (string c in q.HideIfAny.AllConditions)
                {
                    if (_availabilityProvider.GetState(c) >= QuestStatusState.Active) return false;
                }
            }
            return true;
        }
    }
}
