namespace Witcher3MapViewer.Core
{
    public class OptionsStore
    {
        private readonly IQuestAvailabilityProvider _availabilityProvider;

        private bool ShowOnlyAvailable;
        private bool ShowComplete;

        public OptionsStore(IQuestAvailabilityProvider availabilityProvider)
        {
            _availabilityProvider = availabilityProvider;
            ShowOnlyAvailable = true;
            ShowComplete = false;
        }

        public bool QuestDisplayPolicy(Quest q)
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

    public class Options
    {
        public bool ShowOnlyAvailable { get; set; }
        public bool ShowComplete { get; set; }

    }
}
