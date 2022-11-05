namespace Witcher3MapViewer.Core
{
    public class WorldSetting
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public double Slope { get; set; }
        public double XIntercept { get; set; }
        public double YIntercept { get; set; }
        public string TileSource { get; set; }
    }

    public class IconSettings
    {
        public string LargeIconPath { get; set; }
        public string SmallIconPath { get; set; }
        public List<IconInfo> IconInfos { get; set; }

    }

    public class IconInfo
    {
        public string Image { get; set; }
        public string InternalName { get; set; }
        public string GroupName { get; set; }
        public List<string> Aliases { get; set; }

    }
}