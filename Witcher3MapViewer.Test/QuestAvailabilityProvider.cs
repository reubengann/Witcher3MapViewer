using Witcher3MapViewer.Core;

namespace Witcher3MapViewer.Test
{
    public class QuestAvailabilityProviderTest
    {
        [Test]
        public void ReturnsTrueOnAnyQuestWithNoDependencies()
        {
            QuestAvailabilityProvider provider = new QuestAvailabilityProvider();
            Quest q = new Quest
            {
                Name = "foo",
                LevelRequirement = 0,
                QuestType = QuestType.Main,
                World = "WO",
                UniqueID = 2,
                GUID = ""
            };
            Assert.That(provider.IsQuestAvailable(q), Is.True);
        }
    }
}
