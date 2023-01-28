using SaveFile;

Console.WriteLine("Looking for save folder");
string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
string ersatzpath = Path.Combine(myDocuments, "The Witcher 3", "gamesaves");
if(Directory.Exists(ersatzpath))
    Console.WriteLine($"Found at {ersatzpath}");
DirectoryInfo directory = new DirectoryInfo(ersatzpath);
FileInfo myFile = (from f in directory.GetFiles("*.sav")
                   orderby f.LastWriteTime descending
                   select f).First();
Console.WriteLine(myFile.FullName);
Witcher3SaveFile saveFile = new Witcher3SaveFile(myFile.FullName, Witcher3ReadLevel.Quick);
Console.WriteLine("Success");
Console.WriteLine($"Player level is {saveFile.CharacterLevel}");
foreach(Witcher3JournalEntryStatus f in saveFile.CJournalManager.Statuses)
{
    Console.WriteLine($"{f.PrimaryGUID}: {f.Status}");
}
