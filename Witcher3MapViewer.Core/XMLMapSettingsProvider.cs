using System.Xml.Serialization;

namespace Witcher3MapViewer.Core
{
    public class XMLMapSettingsProvider : IMapSettingsProvider
    {
        Dictionary<string, WorldSetting>? WorldSettings;
        private IconSettings _iconSettings;

        public XMLMapSettingsProvider(Stream s)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ApplicationSettingRootDAO));
            ApplicationSettingRootDAO? readitems = (ApplicationSettingRootDAO?)serializer.Deserialize(s);
            if (readitems == null) throw new Exception();
            if (readitems.worldsettings != null)
            {
                WorldSettings = new Dictionary<string, WorldSetting>();
                foreach (var item in readitems.worldsettings.Worlds)
                {
                    WorldSettings[item.ShortName] = new WorldSetting
                    {
                        Name = item.ShortName,
                        ShortName = item.ShortName,
                        Slope = item.conversionsetting.Slope,
                        XIntercept = item.conversionsetting.xintercept,
                        YIntercept = item.conversionsetting.yintercept,
                        TileSource = item.filename
                    };
                }
            }
            if (readitems.iconsettings != null)
            {
                _iconSettings = new IconSettings
                {
                    SmallIconPath = readitems.iconsettings.SmallIconPath,
                    LargeIconPath = readitems.iconsettings.LargeIconPath,
                    IconInfos = readitems.iconsettings.Icons.Select(x => new IconInfo
                    {
                        GroupName = x.Groupname,
                        Image = x.ImageName,
                        InternalName = x.InternalName,
                        Aliases = x.Aliases
                    }).ToList()
                };
            }
        }

        public IconSettings GetIconSettings()
        {
            return _iconSettings;
        }

        public WorldSetting GetWorldSetting(string worldShortName)
        {
            return WorldSettings[worldShortName];
        }
    }


    [XmlRoot("settings")]
    public class ApplicationSettingRootDAO
    {
        [XmlElement("worldsettings")]
        public WorldSettingsDAO worldsettings { get; set; }

        [XmlElement("iconsettings")]
        public IconSettingsDAO iconsettings { get; set; }
    }

    public class WorldSettingsDAO
    {
        [XmlElement("worldsetting")]
        public List<WorldSettingDAO> Worlds { get; set; }
    }

    public class IconSettingsDAO
    {
        [XmlElement("icon")]
        public List<IconSettingDAO> Icons { get; set; }

        [XmlElement("largeiconpath")]
        public string LargeIconPath { get; set; }

        [XmlElement("smalliconpath")]
        public string SmallIconPath { get; set; }
    }

    public class IconSettingDAO
    {
        [XmlElement("image")]
        public string ImageName { get; set; }

        [XmlElement("internalname")]
        public string InternalName { get; set; }

        [XmlElement("alias")]
        public List<string> Aliases { get; set; }

        [XmlElement("groupname")]
        public string Groupname { get; set; }
    }

    public class WorldSettingDAO
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("shortname")]
        public string ShortName { get; set; }

        [XmlElement("alias")]
        public string AliasShortName { get; set; }

        [XmlElement("conversion")]
        public ConversionSettingDAO conversionsetting { get; set; }

        [XmlElement("tilesource")]
        public string filename { get; set; }
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
