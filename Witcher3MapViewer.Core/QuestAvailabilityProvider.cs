namespace Witcher3MapViewer.Core
{
    public class QuestAvailabilityProvider : IQuestAvailabilityProvider
    {
        public bool IsQuestAvailable(Quest q)
        {
            return true;
        }
    }
}