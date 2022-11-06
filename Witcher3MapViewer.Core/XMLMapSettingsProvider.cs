using System.Xml.Serialization;

namespace Witcher3MapViewer.Core
{
    public class XMLMapSettingsProvider : IMapSettingsProvider
    {
        Dictionary<string, WorldSetting>? WorldSettings;
        private IconSettings? _iconSettings;
        private List<WorldSetting> _allSettings;

        public XMLMapSettingsProvider(Stream s)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ApplicationSettingRootDAO));
            ApplicationSettingRootDAO? readitems = (ApplicationSettingRootDAO?)serializer.Deserialize(s);
            _allSettings = new List<WorldSetting>();
            if (readitems == null) throw new Exception();
            if (readitems.worldsettings != null && readitems.worldsettings.Worlds != null)
            {
                WorldSettings = new Dictionary<string, WorldSetting>();
                foreach (WorldSettingDAO? item in readitems.worldsettings.Worlds)
                {
                    WorldSetting world = new WorldSetting
                    {
                        Name = item.Name,
                        ShortName = item.ShortName,
                        Slope = item.conversionsetting.Slope,
                        XIntercept = item.conversionsetting.xintercept,
                        YIntercept = item.conversionsetting.yintercept,
                        TileSource = item.filename
                    };
                    WorldSettings[item.ShortName] = world;
                    _allSettings.Add(world);
                }
            }
            if (readitems.iconsettings != null)
            {
                _iconSettings = new IconSettings
                {
                    SmallIconPath = readitems.iconsettings.SmallIconPath ?? "",
                    LargeIconPath = readitems.iconsettings.LargeIconPath ?? "",
                    IconInfos = readitems.iconsettings?.Icons?.Select(x => new IconInfo
                    {
                        GroupName = x.Groupname,
                        Image = x.ImageName,
                        InternalName = x.InternalName,
                        Aliases = x.Aliases ?? new List<string>()
                    }).ToList() ?? new List<IconInfo>()
                };
            }


        }

        public IconSettings GetIconSettings()
        {
            if (_iconSettings == null) throw new Exception();
            return _iconSettings;
        }

        public WorldSetting GetWorldSetting(string worldShortName)
        {
            if (WorldSettings == null) throw new Exception();
            return WorldSettings[worldShortName];
        }

        public static XMLMapSettingsProvider FromFile(string filepath)
        {
            using FileStream sr = new FileStream(filepath, FileMode.Open);
            return new XMLMapSettingsProvider(sr);
        }

        public List<WorldSetting> GetAll()
        {
            return _allSettings;
        }
    }


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
