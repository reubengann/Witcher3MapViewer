using System.Text;
using Witcher3MapViewer.Core;

namespace Witcher3MapViewer.Test
{
    public class XMLQuestListProviderTests
    {
        string mockwithOne = @"<quests><quest type=""main"" level=""24"" world=""WO"" id=""2"">
                            <name>Kaer Morhen</name>
                            <GUID>C1AA4441-48FF64E8-972B3BB9-250CC191</GUID>
                            <reward XP=""250""/>
                            </quest></quests>";

        string mockWithDependency = @"<quests>
                <quest type=""main"" level=""0"" world=""WO"" id=""2"">
                    <name>Quest 1</name>
                    <GUID>C1AA4441-48FF64E8-972B3BB9-250CC191</GUID>
                    <reward XP=""250""/>
                </quest>

                <quest type=""side"" level=""0"" world = ""WO"" id=""19"">
                    <name>Quest 2</name>
                    <available>
                        <GUID state=""JS_Success"">C1AA4441-48FF64E8-972B3BB9-250CC191</GUID>
                    </available>
                    <discoverprompt>
                        <info>! sign over a man talking to a dog</info>
                        <XY world=""WO"" X=""476"" Y=""-106"" />
                    </discoverprompt>
                    <GUID>3B6DBB8A-4E5095A7-8DA31EB3-4F2C4D1F</GUID>
                    <reward XP=""25""/>
                </quest>
                </quests>";

        string mockWithObjectives = @"<quests>
<quest type=""main"" level=""0"" world = ""WO"" id=""3"">
<name>Lilac and Gooseberries</name>
<GUID>768A197C-4630DC8A-735A4BB3-C9535B0B</GUID>
<available>
<GUID state=""JS_Success"">C1AA4441-48FF64E8-972B3BB9-250CC191</GUID>
</available>
<reward XP=""400""/>
<objectives>
<objective id=""438"">
<name>Ask travelers about Yennefer</name>
<GUID>42CE0F47-454E0055-8AC991AD-A09CAFB3</GUID>
</objective>
</objectives>
<subquests>
<quest type=""main"" level=""3"" world = ""WO"" id=""44"">
<name>The Beast of White Orchard</name>
<GUID>F6086B74-4E68EDFF-D79CE191-13BA33A7</GUID>
<available>
<GUID>42CE0F47-454E0055-8AC991AD-A09CAFB3</GUID>
</available>
<objectives>
<objective id=""439"">
<name>Ask the herbalist about buckthorn</name>
<GUID>50E659DB-44831AA8-31BA8EA6-8EB62F91</GUID>
</objective>
</objectives>
<reward XP=""500""/>
</quest>
<quest type=""main"" level=""2"" world = ""WO"" id=""39"">
<name>The Incident at White Orchard</name>
<GUID>03E485C6-4719EFBD-29B7288E-2145E02E</GUID>
<available>
<GUID state=""JS_Success"">42CE0F47-454E0055-8AC991AD-A09CAFB3</GUID>
</available>
<reward XP=""300""/>
</quest>
</subquests>
</quest>
</quests>";

        string mockWithHide = @"<quests><quest type=""side"" world=""VE"" level=""25"" id=""267"">
<name>Take What You Want</name>
<GUID>7258B2FB-4A41EF24-647735A2-6528FC9F</GUID>
<available>
<GUID state=""JS_Success"">BD40C6D4-4532FB2D-187DEDBF-2C42C6F6</GUID>
</available>
<hideif>
<GUID state=""JS_Active"">3D38A3BB-43644405-D5D87083-7731411C</GUID>
</hideif>
</quest><outcome id=""450"">
<name>Killed Gaetan in Where the Cat and Wolf Play</name>
<GUID>3D38A3BB-43644405-D5D87083-7731411C</GUID>
</outcome></quests>";

        string mockWithStrict = @"<quests><quest type=""side"" world=""NO, SK"" level=""11"" id=""133"">
<name>Following the Thread</name>
<GUID>23EBF49C-4CD2BFB5-A2D8FB94-B27040A6</GUID>
<available>
<GUID state=""JS_Success"">550442BD-43AF7D49-9E0A09B5-54BCCDCC</GUID>
</available>
<strict>
<GUID state=""JS_Success"">75CE5700-4B1F1BBE-069237AE-6D20CA0F</GUID>
</strict>
<strict>
<GUID state=""JS_Success"">130A322C-479E4E1C-6C0ED1B4-0180136D</GUID>
</strict>
<discoverprompt>
<XY world=""NO"" X=""667"" Y=""1898""/>
<info>Notice board (Monster in the Bits)</info>
</discoverprompt>
</quest></quests>";

        string mockWithAuto = @"<quests>
<quest type=""contract"" world=""VE"" level=""25"" id=""266"">
<name>The Beast of Honorton</name>
<GUID>BD40C6D4-4532FB2D-187DEDBF-2C42C6F6</GUID>
<available>
<GUID state=""JS_Success"">3A6B433B-47EA9285-23D96FB9-9A37E56E</GUID>
</available>
<discoverprompt>
<XY world=""VE"" X=""197"" Y=""-456""/>
<info>Notice board</info>
</discoverprompt>
<setdoneautomatic>
<GUID state=""JS_Success"">35CCCB0A-4C518790-D11C34B4-6CDBEA77</GUID>
</setdoneautomatic>
</quest>
</quests>";

        [Test]
        public void WhenOnlyOneQuestReadsOne()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mockwithOne));
            var reader = new XMLQuestListProvider(ms);
            Assert.That(reader.GetAllQuests().Count, Is.EqualTo(1));
        }

        [Test]
        public void WhenHasOneQuestReadsFields()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mockwithOne));
            var reader = new XMLQuestListProvider(ms);
            var result = reader.GetAllQuests()[0];
            Assert.That(result.QuestType, Is.EqualTo(QuestType.Main));
            Assert.That(result.UniqueID, Is.EqualTo(2));
            Assert.That(result.LevelRequirement, Is.EqualTo(24));
            Assert.That(result.World, Is.EqualTo("WO"));
            Assert.That(result.Name, Is.EqualTo("Kaer Morhen"));
            Assert.That(result.GUID, Is.EqualTo("C1AA4441-48FF64E8-972B3BB9-250CC191"));
            Assert.That(result.Reward.XP, Is.EqualTo(250));
        }

        [Test]
        public void CanReadSideQuestFields()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mockWithDependency));
            var reader = new XMLQuestListProvider(ms);
            var result = reader.GetAllQuests()[1];
            Assert.That(result.QuestType, Is.EqualTo(QuestType.SideQuest));
            Assert.That(result.AvailableIfAny.Success[0], Is.EqualTo("C1AA4441-48FF64E8-972B3BB9-250CC191"));
            Assert.That(result.DiscoverPrompt.Info, Is.EqualTo("! sign over a man talking to a dog"));
            Assert.That(result.DiscoverPrompt.Location.WorldCode, Is.EqualTo("WO"));
            Assert.That(result.DiscoverPrompt.Location.X, Is.EqualTo(476));
            Assert.That(result.DiscoverPrompt.Location.Y, Is.EqualTo(-106));
        }

        [Test]
        public void ReadsObjectives()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mockWithObjectives));
            var reader = new XMLQuestListProvider(ms);
            var result = reader.GetAllQuests()[0];
            Assert.That(result.Objectives.Count, Is.EqualTo(1));
            Assert.That(result.Subquests.Count, Is.EqualTo(2));
        }

        [Test]
        public void WhenHasHideConditionsReadsThem()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mockWithHide));
            var reader = new XMLQuestListProvider(ms);
            var result = reader.GetAllQuests()[0];
            Assert.That(result.HideIfAny.Active.Count, Is.EqualTo(1));
        }

        [Test]
        public void ReadsStrictConditions()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mockWithStrict));
            var reader = new XMLQuestListProvider(ms);
            var result = reader.GetAllQuests()[0];
            Assert.That(result.RequiredStrictConditions.Success.Count, Is.EqualTo(2));
        }

        [Test]
        public void ReadsAutoDone()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mockWithAuto));
            var reader = new XMLQuestListProvider(ms);
            var result = reader.GetAllQuests()[0];
            Assert.That(result.AutomaticallyDoneIfConditions.Success.Count, Is.EqualTo(1));
        }

        [Test]
        public void WhenHasOutcomeLinkAddsIt()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mockWithHide));
            var reader = new XMLQuestListProvider(ms);
            var result = reader.GetAllQuests()[0];
            Assert.That(reader.FindAdvent(result.HideIfAny.Active[0]).GUID, Is.EqualTo("3D38A3BB-43644405-D5D87083-7731411C"));
        }
    }
}
