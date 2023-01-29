using Moq;
using Witcher3MapViewer.Core;

namespace Witcher3MapViewer.Test
{
    internal class QuestListViewModelTest
    {
        Quest makeQuest(QuestType t, int levelRequired)
        {
            return new Quest
            {
                Name = "First Quest",
                LevelRequirement = levelRequired,
                QuestType = t,
                World = "WO",
                UniqueID = 2,
                GUID = Guid.NewGuid().ToString(),
            };
        }

        [Test]
        public void Test_When_No_Requirement_SelectBest_Sets_SideQuest_as_Best()
        {
            List<Quest> quests = new List<Quest> { makeQuest(QuestType.Main, 0), makeQuest(QuestType.SideQuest, 0) };
            QuestListViewModel vm = new QuestListViewModel(quests, new Mock<IQuestAvailabilityProvider>().Object);
            vm.SelectBest();
            Assert.That(vm.CurrentQuests[1].IsSelected, Is.True);
        }
    }
}
