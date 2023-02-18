using SaveFile;
string filename;
if (args.Length > 0)
{
    filename = args[0];
    Console.WriteLine($"Opening {filename}");
    if (!File.Exists(filename))
    {
        Console.WriteLine($"Cannot find filename {filename}");
        return;
    }
}
else
{
    Console.WriteLine("Looking for save folder");
    string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    string ersatzpath = Path.Combine(myDocuments, "The Witcher 3", "gamesaves");
    if (Directory.Exists(ersatzpath))
        Console.WriteLine($"Found at {ersatzpath}");
    DirectoryInfo directory = new DirectoryInfo(ersatzpath);
    FileInfo myFile = (from f in directory.GetFiles("*.sav")
                       orderby f.LastWriteTime descending
                       select f).First();
    Console.WriteLine(myFile.FullName);
    filename = myFile.FullName;
}
Witcher3SaveFile saveFile = new Witcher3SaveFile(filename, Witcher3ReadLevel.Quick);
Console.WriteLine("Success");
Console.WriteLine($"Player level is {saveFile.CharacterLevel}");
foreach (Witcher3JournalEntryStatus f in saveFile.CJournalManager.Statuses)
{
    Console.WriteLine($"{f.PrimaryGUID}: {f.Status}");
}
