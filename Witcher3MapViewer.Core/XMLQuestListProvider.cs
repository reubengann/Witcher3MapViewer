﻿using System.Xml.Serialization;
using Witcher3MapViewer.Core.DAO;

namespace Witcher3MapViewer.Core
{
    public class XMLQuestListProvider : IQuestListProvider
    {
        private readonly List<Quest> quests;
        private Dictionary<string, Advent> Index;

        public List<Quest> GetAllQuests()
        {
            return quests;
        }

        public XMLQuestListProvider(Stream s)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(QuestListDAO));
            QuestListDAO? readitems = (QuestListDAO?)serializer.Deserialize(s);
            if (readitems == null) throw new Exception();
            Index = new Dictionary<string, Advent>();

            quests = new List<Quest>();

            if (readitems.Quests != null)
            {
                foreach (QuestDAO? item in readitems.Quests)
                {
                    Quest quest = ExtractQuest(item);

                    quests.Add(quest);
                    Index[quest.GUID] = quest;
                }
            }
            if (readitems.Outcomes != null)
            {
                foreach (QuestDAO? item in readitems.Outcomes)
                {
                    Outcome outcome = new Outcome
                    {
                        GUID = item.GUID.Value,
                        UniqueID = item.UniqueID,
                        Name = item.Name
                    };
                    Index[outcome.GUID] = outcome;

                }
            }
        }



        private Quest ExtractQuest(QuestDAO item)
        {
            Quest quest = new Quest
            {
                UniqueID = item.UniqueID,
                QuestType = GetQuestType(item.QuestType),
                LevelRequirement = item.LevelRequirement,
                World = item.World,
                Name = item.Name,
                GUID = item.GUID?.Value ?? "",

            };
            if (item.Reward != null)
            {
                quest.Reward = new QuestReward { XP = item.Reward.RewardXP };
            }
            if (item.AvailableConditions != null)
            {
                foreach (var condition in item.AvailableConditions)
                {
                    foreach (var reference in condition.GUIDStates)
                    {
                        if (reference.ActiveState == QuestStatusState.Success)
                        {
                            quest.AvailableIfAny.Success.Add(reference.Value);
                        }
                        else if (reference.ActiveState == QuestStatusState.NotFound)
                        {
                            quest.AvailableIfAny.Any.Add(reference.Value);
                        }
                        else if (reference.ActiveState == QuestStatusState.Active)
                        {
                            quest.AvailableIfAny.Active.Add(reference.Value);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
            }
            if (item.DiscoverPrompt != null)
            {
                QuestDiscoverPromptDAO prompt = item.DiscoverPrompt;
                quest.DiscoverPrompt = new QuestDiscoverPrompt
                {
                    Info = prompt.Info,

                };
                if (prompt.DiscoverPosition != null)
                    quest.DiscoverPrompt.Location = new QuestDiscoverLocation
                    {
                        WorldCode = prompt.DiscoverPosition.World,
                        X = prompt.DiscoverPosition.X,
                        Y = prompt.DiscoverPosition.Y
                    };
            }
            if (item.ObjectivesAsRead != null)
            {
                quest.Objectives = new List<Quest>();
                foreach (var objective in item.ObjectivesAsRead)
                {
                    quest.Objectives.Add(new Quest
                    {
                        Name = objective.Name,
                        GUID = objective.GUID.Value,
                    });
                }
            }
            if (item.SubquestsAsRead != null)
            {
                foreach (var subquestDAO in item.SubquestsAsRead)
                {
                    var subquest = ExtractQuest(subquestDAO);
                    quest.Subquests.Add(subquest);
                }
            }
            if (item.HideConditions != null)
            {
                quest.HideIfAny = new QuestConditions();
                foreach (var c in item.HideConditions)
                {
                    if (c.ActiveState == QuestStatusState.Active)
                        quest.HideIfAny.Active.Add(c.Value);
                    else if (c.ActiveState == QuestStatusState.Success)
                        quest.HideIfAny.Success.Add(c.Value);
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            if (item.StrictConditions != null)
            {
                foreach (var c in item.StrictConditions)
                {
                    foreach (var g in c.GUIDStates)
                    {
                        if (g.ActiveState == QuestStatusState.Success)
                            quest.RequiredStrictConditions.Success.Add(g.Value);
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
            }
            if (item.AutomaticConditions != null)
            {
                foreach (var ac in item.AutomaticConditions)
                {
                    if (ac.ActiveState == QuestStatusState.Success)
                    {
                        quest.AutomaticallyDoneIfConditions.Success.Add(ac.Value);
                    }
                    else throw new NotImplementedException();
                }
            }
            return quest;
        }

        private QuestType GetQuestType(string typeName)
        {
            switch (typeName)
            {
                case "main":
                    return QuestType.Main;
                case "dlcmain":
                    return QuestType.DLCMain;
                case "side":
                    return QuestType.SideQuest;
                case "contract":
                    return QuestType.Contract;
                case "treasure":
                    return QuestType.Treasure;
                case "event":
                    return QuestType.Event;
                case "race":
                    return QuestType.Race;
                case "endgame":
                    return QuestType.EndGame;
                default:
                    throw new NotImplementedException();
            }
        }

        public static XMLQuestListProvider FromFile(string filepath)
        {
            using FileStream sr = new FileStream(filepath, FileMode.Open);
            return new XMLQuestListProvider(sr);
        }

        public Advent FindAdvent(string guid)
        {
            return Index[guid];
        }
    }
}
