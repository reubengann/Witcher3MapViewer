using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Witcher3MapViewer.Readers
{
    static class GameStatusSaver
    {        

        public static void Save(string filename, List<QuestViewModel> models, int level)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                WriteLevelInfo(sw, level);
                WriteQuestInfo(sw, models);

            }
        }

        private static void WriteLevelInfo(StreamWriter sw, int level)
        {
            sw.WriteLine("[Level]");
            sw.WriteLine(level);
        }

        private static void WriteQuestInfo(StreamWriter sw, List<QuestViewModel> models)
        {
            sw.WriteLine("[Quest]");
            foreach (QuestViewModel qvm in models)
            {
                bool deferred = qvm.IsDeferred ?? false;
                if (deferred || qvm.correspondingQuest.Forced)
                {
                    WriteModelStatus(sw, qvm);
                    sw.Write("\n");
                }
            }
        }

        private static void WriteModelStatus(StreamWriter sw, QuestViewModel qvm)
        {
            sw.Write(qvm.correspondingQuest.UniqueID);
            sw.Write("\t");
            if(qvm.correspondingQuest.GUID != null && qvm.correspondingQuest.GUID.Value != "")
            {
                sw.Write(qvm.correspondingQuest.GUID.Value);
            }
            if (qvm.IsDeferred == true)
                sw.Write("\tD");
            else sw.Write("\tU");
            if(qvm.IsChecked == true)
                sw.Write("\tD");
            else sw.Write("\tU");
        }
    }

    public class GameStatusReader
    {
        public List<Quest> QuestInfo;
        public int PlayerLevel = 1;

        public GameStatusReader(string filename)
        {            
            using (StreamReader sr = new StreamReader(filename))
            {
                if (!ReadUntil(sr, "[Level]"))
                    return;
                int.TryParse(sr.ReadLine(), out PlayerLevel);
                if (!ReadUntil(sr, "[Quest]"))
                    return;
                QuestInfo = ReadQuestInfo(sr);
            }
            
        }

        private static List<Quest> ReadQuestInfo(StreamReader sr)
        {
            List<Quest> output = new List<Quest>();
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line != "")
                {
                    try
                    {
                        output.Add(ReadModelStatus(line));
                    }
                    catch (System.Reflection.AmbiguousMatchException)
                    {
                        return output;
                    }
                }
            }
            return output;
        }

        private static bool ReadUntil(StreamReader sw, string query)
        {
            while (!sw.EndOfStream)
            {
                if (sw.ReadLine().Trim() == query)
                    return true;
            }
            return false;
        }

        private static Quest ReadModelStatus(string line)
        {
            string[] fields = line.Split('\t');
            Quest q = new Quest();
            q.UniqueID = int.Parse(fields[0]);
            q.GUID = new QuestGUIDState();
            q.GUID.Value = fields[1];
            if (fields[2] == "D")
                q.Deferred = true;
            else q.Deferred = false;
            if (fields[3] == "D")
                q.Done = true;
            else q.Done = false;
            return q;
        }
    }
}
