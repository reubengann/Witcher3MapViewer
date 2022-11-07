namespace Witcher3MapViewer.Core
{
    public interface GameStatusProvider
    {
        QuestStatusState GetQuestStatusState(string guid);
        void SetQuestStatusState(string guid, QuestStatusState state);
    }
}
