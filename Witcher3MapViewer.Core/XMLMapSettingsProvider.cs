using System.Xml.Serialization;
using Witcher3MapViewer.Core.DAO;

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


}
