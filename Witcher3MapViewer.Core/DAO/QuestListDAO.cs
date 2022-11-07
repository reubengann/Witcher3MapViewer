#nullable disable
using System.Xml.Serialization;

namespace Witcher3MapViewer.Core.DAO
{
    public enum QuestStatusState
    {
        NotFound = 0, Inactive = 1, Active = 2, Failed = 3, Success = 4
    }

    [XmlRoot("quests")]
    public class QuestListDAO
    {
        [XmlElement("quest")]
        public List<QuestDAO> Quests { get; set; }

        [XmlElement("outcome")]
        public List<QuestDAO> Outcomes { get; set; }
    }

    public class QuestDAO
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlAttribute("type")]
        public string QuestType { get; set; }

        [XmlAttribute("level")]
        public int LevelRequirement { get; set; }

        [XmlAttribute("world")]
        public string World { get; set; }

        [XmlAttribute("id")]
        public int UniqueID { get; set; }

        [XmlElement("discovered")]
        public List<QuestConditionDAO> DiscoveredConditions { get; set; }

        [XmlElement("available")]
        public List<QuestConditionDAO> AvailableConditions { get; set; }

        [XmlElement("strict")]
        public List<QuestConditionDAO> StrictConditions { get; set; }

        [XmlArray("subquests"), XmlArrayItem("quest")]
        public QuestDAO[] SubquestsAsRead { get; set; }

        [XmlElement("reward")]
        public QuestRewardDAO Reward { get; set; }

        [XmlElement("GUID")]
        public QuestGUIDStateDAO GUID { get; set; }

        [XmlArray("objectives"), XmlArrayItem("objective")]
        public QuestObjectiveDAO[] ObjectivesAsRead { get; set; }

        [XmlElement("discoverprompt")]
        public QuestDiscoverPromptDAO DiscoverPrompt { get; set; }

        [XmlArray("setdoneautomatic"), XmlArrayItem("GUID")]
        public List<QuestGUIDStateDAO> AutomaticConditions { get; set; }

        [XmlArray("hideif"), XmlArrayItem("GUID")]
        public List<QuestGUIDStateDAO> HideConditions { get; set; }
    }

    public class QuestConditionDAO
    {
        [XmlElement("GUID")]
        public List<QuestGUIDStateDAO> GUIDStates { get; set; }
    }

    public class QuestGUIDStateDAO
    {
        [XmlIgnore]
        public QuestStatusState ActiveState;

        [XmlAttribute("state")]
        public string ActiveStateProxy
        {
            get { return ActiveState.ToString(); }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    ActiveState = default(QuestStatusState);
                }
                else
                {
                    switch (value)
                    {
                        case "JS_Success":
                            ActiveState = QuestStatusState.Success;
                            break;
                        case "JS_Failed":
                            ActiveState = QuestStatusState.Failed;
                            break;
                        case "JS_Inactive":
                            ActiveState = QuestStatusState.Inactive;
                            break;
                        case "JS_Active":
                            ActiveState = QuestStatusState.Active;
                            break;
                        default:
                            ActiveState = QuestStatusState.NotFound;
                            break;
                    }
                }
            }
        }

        [XmlText]
        public string Value { get; set; }
    }

    public class QuestRewardDAO
    {
        [XmlAttribute("XP")]
        public int RewardXP { get; set; }
    }

    public class QuestDiscoverPositionDAO
    {
        [XmlAttribute("X")]
        public int X { get; set; }

        [XmlAttribute("Y")]
        public int Y { get; set; }

        [XmlAttribute("world")]
        public string World { get; set; }
    }

    public class QuestDiscoverPromptDAO
    {
        [XmlElement("XY")]
        public QuestDiscoverPositionDAO DiscoverPosition { get; set; }

        [XmlElement("info")]
        public string Info;
    }

    public class QuestObjectiveDAO
    {
        [XmlAttribute("id")]
        public int UniqueID { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("GUID")]
        public QuestGUIDStateDAO GUID { get; set; }
    }
}
