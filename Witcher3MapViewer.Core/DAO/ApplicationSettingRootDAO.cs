using System.Xml.Serialization;

namespace Witcher3MapViewer.Core.DAO
{
    [XmlRoot("settings")]
    public class ApplicationSettingRootDAO
    {
        [XmlElement("worldsettings")]
        public WorldSettingsDAO? worldsettings { get; set; }

        [XmlElement("iconsettings")]
        public IconSettingsDAO? iconsettings { get; set; }
    }

    public class WorldSettingsDAO
    {
        [XmlElement("worldsetting")]
        public List<WorldSettingDAO>? Worlds { get; set; }
    }

    public class IconSettingsDAO
    {
        [XmlElement("icon")]
        public List<IconSettingDAO>? Icons { get; set; }

        [XmlElement("largeiconpath")]
        public string? LargeIconPath { get; set; }

        [XmlElement("smalliconpath")]
        public string? SmallIconPath { get; set; }
    }

    public class IconSettingDAO
    {
        [XmlElement("image")]
        public string ImageName { get; set; } = default!;

        [XmlElement("internalname")]
        public string InternalName { get; set; } = default!;

        [XmlElement("alias")]
        public List<string>? Aliases { get; set; }

        [XmlElement("groupname")]
        public string Groupname { get; set; } = default!;
    }

    public class WorldSettingDAO
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = default!;

        [XmlAttribute("shortname")]
        public string ShortName { get; set; } = default!;

        [XmlElement("alias")]
        public string? AliasShortName { get; set; }

        [XmlElement("conversion")]
        public ConversionSettingDAO conversionsetting { get; set; } = default!;

        [XmlElement("tilesource")]
        public string filename { get; set; } = default!;
    }

    public class ConversionSettingDAO
    {
        [XmlAttribute("slope")]
        public double Slope { get; set; }

        [XmlAttribute("xi")]
        public double xintercept { get; set; }

        [XmlAttribute("yi")]
        public double yintercept { get; set; }
    }
}
