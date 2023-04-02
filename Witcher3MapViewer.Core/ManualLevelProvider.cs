using System.Text.Json;

namespace Witcher3MapViewer.Core
{
    public class ManualLevelProvider : ILevelProvider
    {
        private readonly string filename;
        private int Level;
        public event Action LevelChanged;
        public ManualLevelProvider(string filename)
        {
            this.filename = filename;
            if (File.Exists(filename))
            {
                var file = JsonSerializer.Deserialize<CharacterLevelFile>(File.ReadAllText(filename));
                if (file == null) throw new Exception("Bad level file format");
                Level = file.Level;
            }
            else
            {
                Level = 1;
                File.WriteAllText(filename, JsonSerializer.Serialize(new CharacterLevelFile { Level = 1 }));
            }
        }

        public int GetLevel()
        {
            return Level;
        }

        public void SetLevel(int level)
        {
            Level = level;
            File.WriteAllText(filename, JsonSerializer.Serialize(new CharacterLevelFile { Level = level }));
            LevelChanged?.Invoke();
        }
    }

    class CharacterLevelFile
    {
        public int Level { get; set; }
    }
}
