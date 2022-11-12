using Witcher3MapViewer.Core;

namespace Witcher3MapViewer.Test
{
    public class QuestAvailabilityProviderTest
    {
        QuestAvailabilityProvider provider;

        Quest questWithNoDependency = new Quest
        {
            Name = "First Quest",
            LevelRequirement = 0,
            QuestType = QuestType.Main,
            World = "WO",
            UniqueID = 2,
            GUID = "0AE82268-4FFF5D8D-4A219CB4-11E480F8"
        };

        Quest questWithDependency = new Quest
        {
            Name = "Second Quest",
            LevelRequirement = 0,
            QuestType = QuestType.Main,
            World = "WO",
            UniqueID = 3,
            GUID = "C651CB60-4D784070-4F8AD0B6-36D4289A",
        };

        [SetUp]
        public void SetUp()
        {
            provider = new QuestAvailabilityProvider();
            questWithDependency.AvailableIfAny.Success.Add("0AE82268-4FFF5D8D-4A219CB4-11E480F8");
        }

        [Test]
        public void ReturnsTrueOnAnyQuestWithNoDependencies()
        {
            Assert.That(provider.IsQuestAvailable(questWithNoDependency), Is.True);
        }

        [Test]
        public void QuestWithUnmetDependenciesReturnsFalse()
        {
            Assert.That(provider.IsQuestAvailable(questWithDependency), Is.False);
        }

        [Test]
        public void QuestWithMetDependenciesReturnsTrue()
        {
            provider.SetState(questWithNoDependency.GUID, QuestStatusState.Success);
            Assert.That(provider.IsQuestAvailable(questWithDependency), Is.True);
        }

        [Test]
        public void WhenStateIsActiveButSuccessNeededReturnsFalse()
        {
            provider.SetState(questWithNoDependency.GUID, QuestStatusState.Active);
            Assert.That(provider.IsQuestAvailable(questWithDependency), Is.False);
        }
    }
}
