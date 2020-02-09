using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Linq;

namespace Witcher3MapViewer
{
    public class Witcher3ProgressStatus
    {
        public List<Quest> Quests;
        public List<Quest> AvailableQuests;
        public List<Quest> UnavailableQuests;
        public Dictionary<int, QuestStatusState> Statuses;
        public Dictionary<string, int> GUIDToUniqueID;
        public Dictionary<int, Quest> QuestLookupByID;
        //public int CharacterXP, CharacterLevel;
        public List<Quest> Outcomes;
        public List<int> OutcomeIDs;

        private bool onlyAccessible, includeEvents, includeRaces, includeTreasure;

        public Witcher3ProgressStatus(string filename, AvailCond Conditions)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(QuestListAsRead));
            List<QuestAsRead> QuestsAsRead;
            List<QuestAsRead> OutcomesAsRead;
            Quests = new List<Quest>();
            Outcomes = new List<Quest>();
            OutcomeIDs = new List<int>();
            QuestLookupByID = new Dictionary<int, Quest>();
            using (Stream reader = new FileStream(filename, FileMode.Open))
            {
                QuestListAsRead readitems = (QuestListAsRead)serializer.Deserialize(reader);
                QuestsAsRead = readitems.Quests;
                OutcomesAsRead = readitems.Outcomes;
            }
            foreach (QuestAsRead questasread in QuestsAsRead)
            {
                Quest q = new Quest(questasread);
                Quests.Add(q);
            }
            foreach (QuestAsRead questasread in OutcomesAsRead)
            {
                Quest q = new Quest(questasread);
                Outcomes.Add(q);
                OutcomeIDs.Add(q.UniqueID);
            }
            SetUpQuests(Conditions);            
        }

        void SetUpQuests(AvailCond Conditions)
        {
            Statuses = new Dictionary<int, QuestStatusState>();
            GUIDToUniqueID = new Dictionary<string, int>();
            CompileTables();
            AvailableQuests = new List<Quest>();
            UnavailableQuests = new List<Quest>();            
            foreach (Quest q in Quests)            
                UnavailableQuests.Add(q);            
        }

        public void Reset()
        {
            foreach (Quest q in Quests)
                ResetQuest(q);
            foreach (Quest q in Outcomes)
                ResetQuest(q);
            Statuses.Clear();
            GUIDToUniqueID.Clear();            
            QuestLookupByID.Clear();
            CompileTables();
            AvailableQuests.Clear();
            UnavailableQuests.Clear();
            
            foreach (Quest q in Quests)            
                UnavailableQuests.Add(q);
            
        }

        private void ResetQuest(Quest q)
        {
            q.Forced = false;
            q.Done = false;
            q.Deferred = false;
            q.Status = QuestStatusState.NotFound;
            
            foreach(Quest sq in q.Subquests)            
                ResetQuest(sq);
        }

        void CompileTables()
        {
            foreach (Quest q in Quests)
                AddQuestMembersToTables(q);

            foreach (Quest q in Outcomes)
            {
                if (q.UniqueID == 0)
                    throw new Exception("Null ID in outcome " + q.Name);

                if (q.GUID == null)
                    throw new Exception("Outcome " + q.Name + "must have primary GUID");

                Statuses[q.UniqueID] = QuestStatusState.NotFound;
                GUIDToUniqueID[q.GUID.Value] = q.UniqueID;
            }
        }

        void AddQuestMembersToTables(Quest q)
        {
            if (q.UniqueID == 0)
                throw new Exception("Null ID in quest " + q.Name);

            if (QuestLookupByID.ContainsKey(q.UniqueID))
                throw new Exception("duplicate uniqueid!");

            QuestLookupByID[q.UniqueID] = q;

            if (q.AvailableConditions.Count == 0)
            {
                Statuses[q.UniqueID] = QuestStatusState.Active;
                q.Status = QuestStatusState.Active;
            }

            else
            {
                Statuses[q.UniqueID] = QuestStatusState.NotFound;
                q.Status = QuestStatusState.NotFound;
            }

            if (q.GUID != null)
                GUIDToUniqueID[q.GUID.Value] = q.UniqueID;

            if (q.Subquests.Count != 0)
            {
                foreach (Quest sq in q.Subquests)
                {
                    AddQuestMembersToTables(sq);
                }
            }
        }

        private void RecheckStatuses()
        {
            foreach (Quest q in Quests)
                UpdateQuestStatus(q);

            //Check for conditions that may have cropped up that force a quest to be done.            
            foreach (Quest q in Quests)
            {
                if (!q.Done && AnyAutoconditionsMet(q))
                {
                    q.Done = true;
                    q.Status = QuestStatusState.Success;
                    Statuses[q.UniqueID] = QuestStatusState.Success;
                    q.Forced = false;
                }
            }

            
            foreach (Quest q in Quests)
            {
                //If using a save game, but the user has forced a quest done, observe that change by overriding the save populating
                //if (q.Forced)
                //Statuses[q.UniqueID] = q.Status;
                GetForcedStatusesFromQuest(q);
                //UpdateSubQuests(q);

                //Probably a game was started earlier than the current one
                if (!q.Forced && q.Done && Statuses[q.UniqueID] < QuestStatusState.Success)
                {
                    q.Done = false;
                    q.Status = Statuses[q.UniqueID];
                }
            }
        }

        private void UpdateQuestStatus(Quest q)
        {
            if (q.Forced) return;
            q.Status = Statuses[q.UniqueID];
            if (q.Status > QuestStatusState.Active)
                q.Done = true;
            foreach (Quest sq in q.Subquests)
                UpdateQuestStatus(sq);
        }



        //private void UpdateSubQuests(Quest q)
        //{
        //    foreach (Quest sq in q.Subquests)
        //        if (Statuses[GUIDToUniqueID[sq.GUID.Value]] == QuestStatusState.Success)
        //            sq.Done = true;
        //}

        private void GetForcedStatusesFromQuest(Quest q)
        {
            if (q.Forced)
                Statuses[q.UniqueID] = q.Status;
            foreach (Quest sq in q.Subquests)
                GetForcedStatusesFromQuest(sq);            
        }

        private bool AnyAutoconditionsMet(Quest q)
        {
            if (q.AutomaticConditions.Count == 0)
                return false;
            foreach (QuestGUIDState state in q.AutomaticConditions)
            {
                var guidStatus = Statuses[GUIDToUniqueID[state.Value]];                
                if (guidStatus >= state.ActiveState && !q.Done)
                    return true;
            }
            return false;
        }

        private void ParseConditions(AvailCond Conditions)
        {
            onlyAccessible = Conditions.HasFlag(AvailCond.Accessible);
            includeEvents = Conditions.HasFlag(AvailCond.Events);
            includeRaces = Conditions.HasFlag(AvailCond.Races);
            includeTreasure = Conditions.HasFlag(AvailCond.Treasure);
        }

        public List<Quest> GetAvailable(AvailCond Conditions)
        {
            ParseConditions(Conditions);
            RecheckStatuses();

            List<Quest> selected = UnavailableQuests.Where(item => DetermineIfAvailable(item)).ToList();
            selected.ForEach(item => UnavailableQuests.Remove(item));
            AvailableQuests.AddRange(selected);
            return selected;
        }

        public List<Quest> CullNewlyUnavailable(AvailCond Conditions)
        {
            ParseConditions(Conditions);
            RecheckStatuses();

            List<Quest> selected = AvailableQuests.Where(item => !DetermineIfAvailable(item)).ToList();
            selected.ForEach(item => AvailableQuests.Remove(item));
            UnavailableQuests.AddRange(selected);
            return selected;
        }

        private bool DetermineIfAvailable(Quest q)
        {
            if (q.Status == QuestStatusState.Success)
                return true;
            if (!onlyAccessible)
                return true;
            if (q.QuestType == "event" && !includeEvents)
                return false;
            if (q.AvailableConditions.Count == 0)
                return true;
            
            if (q.HideConditions != null)
            {
                if (AnyStatesAreMet(q.HideConditions))
                    return false;
            }
            bool AllAreMet = true;
            //if all are met, then this condition is met
            foreach (QuestCondition qc in q.AvailableConditions)
            {
                bool ThisConditionMet = false;
                //if any are met, then this condition is met                
                if (AnyStatesAreMet(qc.GUIDStates))
                {
                    if (q.QuestType == "race")
                        ThisConditionMet = includeRaces;
                    if (q.QuestType == "treasure")
                        ThisConditionMet = includeTreasure;
                    else ThisConditionMet = true;
                }
                AllAreMet = AllAreMet && ThisConditionMet;
            }
            foreach (QuestCondition qc in q.StrictConditions)
            {
                bool ThisConditionMet = false;
                //if any are met, then this condition is met
                foreach (QuestGUIDState qs in qc.GUIDStates)
                {
                    QuestStatusState CurrentStatus = Statuses[GUIDToUniqueID[qs.Value]];
                    QuestStatusState StatusForBeingAvailable = qs.ActiveState;
                    if (CurrentStatus >= StatusForBeingAvailable)
                    {
                        if (q.QuestType == "event")
                            ThisConditionMet = includeEvents;
                        else if (q.QuestType == "race")
                            ThisConditionMet = includeRaces;
                        else if (q.QuestType == "treasure")
                            ThisConditionMet = includeTreasure;
                        else ThisConditionMet = true;
                    }
                }
                AllAreMet = AllAreMet && ThisConditionMet;
            }
            //if any are NOT met, then the quest is not available
            return AllAreMet;
        }

        private bool AnyStatesAreMet(List<QuestGUIDState> Conditions)
        {
            if (Conditions.Count == 0)
                return false;
            foreach (QuestGUIDState s in Conditions)
            {
                if (CheckCondition(s))
                    return true;
            }
            return false;
        }

        private bool CheckCondition(QuestGUIDState hc)
        {
            QuestStatusState CurrentStatus = Statuses[GUIDToUniqueID[hc.Value]];
            QuestStatusState StatusForBeingAvailable = hc.ActiveState;
            if (CurrentStatus >= StatusForBeingAvailable)
            {
                return true;
            }
            return false;
        }

        public enum AvailCond
        {
            Accessible = 1, Events = 2, Races = 4, Treasure = 8
        }

        public void PopulateStatusesFromJournal(Witcher3CJournal journal)
        {
            
            //using (StreamWriter sw = new StreamWriter(@"C:\Users\Reuben\Dropbox\Programs\dump.txt"))
            //{
            //    foreach (Witcher3JournalEntryStatus entry in journal.Statuses)
            //    {
            //        sw.WriteLine(entry.PrimaryGUID + "\t" + entry.Status.ToString());
            //    }
            //}
            
            foreach (int uid in Statuses.Keys.ToList())
                Statuses[uid] = QuestStatusState.NotFound;
            foreach (Witcher3JournalEntryStatus entry in journal.Statuses)
            {
                if (GUIDToUniqueID.ContainsKey(entry.PrimaryGUID))
                {
                    Statuses[GUIDToUniqueID[entry.PrimaryGUID]] = entry.Status;
                    if (entry.Status == QuestStatusState.Success || entry.Status == QuestStatusState.Failed)
                    {
                        int uid = GUIDToUniqueID[entry.PrimaryGUID];
                        if (!OutcomeIDs.Contains(uid))
                        {
                            QuestLookupByID[uid].Done = true;
                            QuestLookupByID[uid].Status = entry.Status;
                        }
                    }
                }
            }
        }

        public Quest getQuestByCoordinates(int searchX, int searchY)
        {
            IEnumerable<Quest> results = Quests.Where(q =>
                q.DiscoverPrompt != null &&
                q.DiscoverPrompt.DiscoverPosition != null &&
                q.DiscoverPrompt.DiscoverPosition.X == searchX &&
                q.DiscoverPrompt.DiscoverPosition.Y == searchY);
            return results.Count() <= 0 ? null : results.First();
        }
    } //end class
} //end namespace
