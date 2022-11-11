namespace Witcher3MapViewer.Core
{
    public interface IQuestListProvider
    {
        List<Quest> GetAllQuests();
        Advent FindAdvent(string guid);
    }
}
