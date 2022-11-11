namespace Witcher3MapViewer.Core
{
    internal interface IQuestAvailabilityProvider
    {
        bool IsQuestAvailable(Quest q);
        void SetState(string guid, QuestStatusState state);
    }
}
