using System.Collections.Generic;
using System.Xml.Serialization;

namespace Witcher3MapViewer
{
    [XmlRoot("quests")]
    public class QuestListAsRead
    {
        [XmlElement("quest")]
        public List<QuestAsRead> Quests { get; set; }

        [XmlElement("outcome")]
        public List<QuestAsRead> Outcomes { get; set; }
    }

    public enum QuestStatusState
    {
        NotFound = 0, Inactive = 1, Active = 2, Failed = 3, Success = 4
    }

    public class QuestAsRead
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
        public List<QuestCondition> DiscoveredConditions { get; set; }

        [XmlElement("available")]
        public List<QuestCondition> AvailableConditions { get; set; }

        [XmlElement("strict")]
        public List<QuestCondition> StrictConditions { get; set; }        

        [XmlArray("subquests"), XmlArrayItem("quest")]
        public QuestAsRead[] SubquestsAsRead { get; set; }

        [XmlElement("reward")]
        public QuestReward Reward { get; set; }

        [XmlElement("GUID")]
        public QuestGUIDState GUID { get; set; }

        [XmlArray("objectives"), XmlArrayItem("objective")]
        public QuestObjective[] ObjectivesAsRead { get; set; }

        [XmlElement("discoverprompt")]
        public QuestDiscoverPrompt DiscoverPrompt { get; set; }

        [XmlArray("setdoneautomatic"), XmlArrayItem("GUID")]
        public List<QuestGUIDState> AutomaticConditions { get; set; }

        [XmlArray("hideif"), XmlArrayItem("GUID")]
        public List<QuestGUIDState> HideConditions { get; set; }
    }

    public class Quest : QuestAsRead
    {
        public QuestStatusState _status;
        public QuestStatusState Status
        {
            get { return _status; }
            set
            {
                _status = value;
                if (value == QuestStatusState.Success || value == QuestStatusState.Failed)
                    Done = true;
                else Done = false;
            }
        }
        public List<Quest> Subquests = new List<Quest>();
        public bool Done;
        public bool Forced = false;
        public bool Deferred = false;

        public Quest()
        {
            Done = false;
        }

        public Quest(QuestAsRead toCopy)
        {
            Name = toCopy.Name;
            QuestType = toCopy.QuestType;
            LevelRequirement = toCopy.LevelRequirement;
            World = toCopy.World;
            UniqueID = toCopy.UniqueID;
            DiscoveredConditions = toCopy.DiscoveredConditions;
            AvailableConditions = toCopy.AvailableConditions;            
            Reward = toCopy.Reward;
            GUID = toCopy.GUID;
            ObjectivesAsRead = toCopy.ObjectivesAsRead;
            Subquests = new List<Quest>();
            if (ObjectivesAsRead != null)
            {
                foreach (QuestObjective qo in ObjectivesAsRead)
                    Subquests.Add(FromObjective(qo));
            }
            SubquestsAsRead = toCopy.SubquestsAsRead;
            if (SubquestsAsRead != null)
            {
                foreach (QuestAsRead sub in SubquestsAsRead)
                {
                    Subquests.Add(new Quest(sub));
                }
            }
            _status = QuestStatusState.NotFound;
            Done = false;
            DiscoverPrompt = toCopy.DiscoverPrompt;
            AutomaticConditions = toCopy.AutomaticConditions;
            StrictConditions = toCopy.StrictConditions;
            HideConditions = toCopy.HideConditions;
        }

        public Quest FromObjective(QuestObjective qo)
        {
            Quest Objective = new Quest();
            Objective.Name = qo.Name;
            Objective.GUID = qo.GUID;
            Objective.UniqueID = qo.UniqueID;
            Objective.LevelRequirement = LevelRequirement;
            Objective.World = World;
            Objective.AvailableConditions = AvailableConditions;
            return Objective;
        }
    }


    public class QuestObjective
    {
        [XmlAttribute("id")]
        public int UniqueID { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("GUID")]
        public QuestGUIDState GUID { get; set; }
    }

    public class QuestCondition
    {
        [XmlElement("GUID")]
        public List<QuestGUIDState> GUIDStates { get; set; }
    }

    public class QuestGUIDState
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

    public class QuestReward
    {
        [XmlAttribute("XP")]
        public int RewardXP { get; set; }
    }

    public class QuestDiscoverPosition
    {
        [XmlAttribute("X")]
        public int X { get; set; }

        [XmlAttribute("Y")]
        public int Y { get; set; }

        [XmlAttribute("world")]
        public string World { get; set; }
    }

    public class QuestDiscoverPrompt
    {
        [XmlElement("XY")]
        public QuestDiscoverPosition DiscoverPosition { get; set; }

        [XmlElement("info")]
        public string Info;
    }
}
