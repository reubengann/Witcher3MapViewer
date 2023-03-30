namespace Witcher3MapViewer.Core
{
    public enum QuestStatusState
    {
        NotFound = 0, Inactive = 1, Active = 2, Failed = 3, Success = 4
    }

    public enum QuestType
    {
        Unknown,
        Main,
        DLCMain,
        SideQuest,
        Contract,
        Treasure,
        Event,
        Race,
        EndGame
    }

    public abstract class Advent
    {
        public int UniqueID { get; set; }
        public string GUID { get; set; } = default!;
        public string Name { get; set; } = default!;
    }

    public class Quest : Advent
    {
        public QuestType QuestType { get; set; }
        public int LevelRequirement { get; set; }
        public string World { get; set; } = default!;
        public QuestReward? Reward { get; set; }
        public QuestConditions AvailableIfAny { get; set; } = new QuestConditions();
        public QuestConditions OneOfAbsolutelyRequired { get; set; } = new QuestConditions();
        public QuestConditions HideIfAny { get; set; } = new QuestConditions();
        public QuestConditions RequiredStrictConditions { get; set; } = new QuestConditions();
        public QuestConditions AutomaticallyDoneIfConditions { get; set; } = new QuestConditions();
        public QuestDiscoverPrompt? DiscoverPrompt { get; set; }
        public List<Quest>? Objectives { get; set; }
        public List<Quest> Subquests { get; set; } = new List<Quest>();

        public bool HasAnyConditions => AvailableIfAny.HasAny || HideIfAny.HasAny || RequiredStrictConditions.HasAny;

    }

    public class Outcome : Advent
    {

    }

    //public class QuestObjective
    //{
    //    public string Name { get; set; } = default!;
    //    public string GUID { get; set; } = default!;

    //}

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

        public bool HasAny => Any.Count > 0 || Success.Count > 0 || Active.Count > 0;
        public List<string> AllConditions => Success.Concat(Any).Concat(Active).ToList();
    }

    public class QuestReward
    {
        public int XP { get; set; }
    }
}
