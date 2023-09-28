using CommandLine;
using SaveFile;

Parser.Default.ParseArguments<ParseOptions, CompareOptions>(args)
       .WithParsed<ParseOptions>(o =>
       {
           string filename = o.filename;
           Console.WriteLine($"Opening {filename}");
           if (!File.Exists(filename))
           {
               Console.WriteLine($"Cannot find filename {filename}");
               return;
           }


           Witcher3SaveFile saveFile = new Witcher3SaveFile(filename, Witcher3ReadLevel.Quick);
           Console.WriteLine("Success");
           Console.WriteLine($"Player level is {saveFile.CharacterLevel}");
           foreach (Witcher3JournalEntryStatus f in saveFile.CJournalManager.Statuses)
           {
               Console.WriteLine($"{f.PrimaryGUID}: {f.Status}");
           }
       })
       .WithParsed<CompareOptions>(o =>
       {
           Console.WriteLine("Looking for save folder");
           string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
           string ersatzpath = Path.Combine(myDocuments, "The Witcher 3", "gamesaves");
           if (Directory.Exists(ersatzpath))
               Console.WriteLine($"Found at {ersatzpath}");

           string l_filepath = Path.Combine(ersatzpath, o.filename1);
           string r_filepath = Path.Combine(ersatzpath, o.filename2);
           if (!File.Exists(l_filepath))
           {
               Console.WriteLine($"Cannot find filename {l_filepath}");
               return;
           }
           if (!File.Exists(r_filepath))
           {
               Console.WriteLine($"Cannot find filename {r_filepath}");
               return;
           }
           Console.WriteLine($"Comparing {l_filepath} to {r_filepath}");
           Witcher3SaveFile l_saveFile = new Witcher3SaveFile(l_filepath, Witcher3ReadLevel.Quick);
           Witcher3SaveFile r_saveFile = new Witcher3SaveFile(r_filepath, Witcher3ReadLevel.Quick);
           var l_statuses = l_saveFile.CJournalManager.StatusDict;
           var r_statuses = r_saveFile.CJournalManager.StatusDict;
           foreach (var stat in l_statuses)
           {
               Witcher3JournalEntryStatus foo = stat.Value;
               if (foo.Status != r_statuses[stat.Key].Status)
               {
                   Console.WriteLine($"Status of {stat.Key} is different! In left is {foo.Status} but in right is {r_statuses[stat.Key].Status}");
               }
           }
       });

[Verb("parse", HelpText = "Parse a save file")]
class ParseOptions
{
    //[Value(0)]
    //public string verb { get; set; }

    [Value(0)]
    public string filename { get; set; }
}

[Verb("compare", HelpText = "Compare two save files")]
class CompareOptions
{
    //[Value(0)]
    //public string verb { get; set; }

    [Value(1)]
    public string filename1 { get; set; }

    [Value(2)]
    public string filename2 { get; set; }
}
//if (args.Length > 0)
//{

//}
