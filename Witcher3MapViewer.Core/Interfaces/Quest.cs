namespace Witcher3MapViewer.Core
{

    public enum QuestType
    {
        Unknown,
        Main,
        SideQuest,
        Contract,
        Treasure,
        Event,
        Race,
        EndGame
    }
    public class Quest
    {
        public string Name { get; set; } = default!;
        public QuestType QuestType { get; set; }
        public int LevelRequirement { get; set; }
        public string World { get; set; } = default!;
        public int UniqueID { get; set; }
        public string GUID { get; set; } = default!;
        public QuestReward? Reward { get; set; }
        public QuestConditions AvailableIfAny { get; set; } = new QuestConditions();
        public QuestConditions HideIfAny { get; set; } = new QuestConditions();
        public QuestConditions RequiredStrictConditions { get; set; } = new QuestConditions();
        public QuestConditions AutomaticallyDoneIfConditions { get; set; } = new QuestConditions();
        public QuestDiscoverPrompt? DiscoverPrompt { get; set; }
        public List<QuestObjective>? Objectives { get; set; }
        public List<Quest>? Subquests { get; set; }

    }

    public class QuestObjective
    {
        public string Name { get; set; } = default!;
        public string GUID { get; set; } = default!;

    }

    public class QuestDiscoverPrompt
    {
        public string Info { get; set; } = default!;
        public QuestDiscoverLocation? Location { get; set; } = default!;
    }

    public class QuestDiscoverLocation
    {
        public string WorldCode { get; set; } = default!;
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class QuestConditions
    {
        public List<string> Success { get; set; } = new List<string>();
        public List<string> Any { get; set; } = new List<string>();
        public List<string> Active { get; internal set; } = new List<string>();
    }

    public class QuestReward
    {
        public int XP { get; set; }
    }
}
