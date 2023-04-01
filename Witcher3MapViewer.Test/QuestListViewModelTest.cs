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
            IQuestAvailabilityProvider avail = new Mock<IQuestAvailabilityProvider>().Object;
            QuestListViewModel vm = new QuestListViewModel(quests, avail, new Mock<ILevelProvider>().Object, new OptionsStore(avail));
            vm.SelectBest();
            Assert.That(vm.CurrentQuests[1].IsSelected, Is.True);
        }

        [Test]
        public void Test_When_No_Requirement_SelectBest_Sets_Lowest_Main_as_Best()
        {
            List<Quest> quests = new List<Quest> { makeQuest(QuestType.Main, 0), makeQuest(QuestType.Main, 1) };
            IQuestAvailabilityProvider avail = new Mock<IQuestAvailabilityProvider>().Object;
            QuestListViewModel vm = new QuestListViewModel(quests, avail, new Mock<ILevelProvider>().Object, new OptionsStore(avail));
            vm.SelectBest();
            Assert.That(vm.CurrentQuests[0].IsSelected, Is.True);
        }

        [Test]
        public void Test_When_Below_Sidequest_Requirement_Sets_MainQuest_as_Best()
        {
            List<Quest> quests = new List<Quest> { makeQuest(QuestType.Main, 1), makeQuest(QuestType.SideQuest, 2) };
            Mock<ILevelProvider> mock = new Mock<ILevelProvider>();
            mock.Setup(x => x.GetLevel()).Returns(1);
            IQuestAvailabilityProvider avail = new Mock<IQuestAvailabilityProvider>().Object;
            QuestListViewModel vm = new QuestListViewModel(quests, avail, mock.Object, new OptionsStore(avail));
            vm.SelectBest();
            Assert.That(vm.CurrentQuests[0].IsSelected, Is.True);
        }
    }
}
