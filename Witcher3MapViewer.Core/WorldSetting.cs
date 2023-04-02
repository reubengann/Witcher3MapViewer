namespace Witcher3MapViewer.Core
{
    public class WorldSetting
    {
        public string Name { get; set; } = default!;
        public string ShortName { get; set; } = default!;
        public double Slope { get; set; }
        public double XIntercept { get; set; }
        public double YIntercept { get; set; }
        public string TileSource { get; set; } = default!;
    }

    public class IconSettings
    {
        public string LargeIconPath { get; set; } = default!;
        public string SmallIconPath { get; set; } = default!;
        public List<IconInfo> IconInfos { get; set; } = default!;

    }

    public class IconInfo
    {
        public string Image { get; set; } = default!;
        public string InternalName { get; set; } = default!;
        public string GroupName { get; set; } = default!;
        public List<string> Aliases { get; set; } = default!;

    }
}