using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace Witcher3MapViewer.Readers
{
    public class ApplicationSettingsReader
    {
        public Dictionary<string, RealToGameSpaceConversion> ConversionLibrary;
        public Dictionary<string, string> PathLibrary;
        public Dictionary<string, MapPinType> TypeLookup;
        public List<MapPinType> PinTypes;
        public Dictionary<string, string> ShortNameLookup;
        public List<string> LongNames;
        public List<string> ShortNames;
        public string largeiconpath, smalliconpath;

        public ApplicationSettingsReader(string appdir, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ApplicationSettingRoot));
            ApplicationSettingRoot readitems;
            using (Stream reader = new FileStream(Path.Combine(appdir, filename), FileMode.Open))
            {
                readitems = (ApplicationSettingRoot)serializer.Deserialize(reader);
            }
            ConversionLibrary = new Dictionary<string, RealToGameSpaceConversion>();
            PathLibrary = new Dictionary<string, string>();
            //IconFiles = new Dictionary<string, string>();
            PinTypes = new List<MapPinType>();
            ShortNameLookup = new Dictionary<string, string>();
            ShortNames = new List<string>();
            LongNames = new List<string>();
            foreach (WorldSetting ws in readitems.worldsettings.Worlds)
            {
                string loc = ws.ShortName;
                ConversionLibrary[loc] = new RealToGameSpaceConversion(ws.conversionsetting.Slope,
                    ws.conversionsetting.xintercept, ws.conversionsetting.yintercept);
                if (!File.Exists(Path.Combine(appdir, ws.filename)))
                    throw new FileNotFoundException("Invalid filename");
                PathLibrary[loc] = Path.Combine(appdir, ws.filename);
                ShortNameLookup[ws.ShortName] = loc;
                if (ws.AliasShortName != null)
                    ShortNameLookup[ws.AliasShortName] = ws.ShortName;
                LongNames.Add(ws.Name);
                ShortNames.Add(ws.ShortName);
            }
            TypeLookup = new Dictionary<string, MapPinType>();
            foreach(IconSetting icset in readitems.iconsettings.Icons)
            {
                MapPinType m = new MapPinType(icset.Groupname, icset.InternalName);
                m.Aliases = icset.Aliases;
                m.IconFile = icset.ImageName;
                TypeLookup[icset.InternalName] = m;
                foreach (string alias in icset.Aliases)
                    TypeLookup[alias] = m;
                PinTypes.Add(m);
            }
            largeiconpath = readitems.iconsettings.LargeIconPath;
            smalliconpath = readitems.iconsettings.SmallIconPath;
        }       

    }

    [XmlRoot("settings")]
    public class ApplicationSettingRoot
    {
        [XmlElement("worldsettings")]
        public WorldSettings worldsettings { get; set; }

        [XmlElement("iconsettings")]
        public IconSettings iconsettings { get; set; }
    }

    public class WorldSettings
    {
        [XmlElement("worldsetting")]
        public List<WorldSetting> Worlds { get; set; }
    }

    public class IconSettings
    {
        [XmlElement("icon")]
        public List<IconSetting> Icons { get; set; }

        [XmlElement("largeiconpath")]
        public string LargeIconPath { get; set; }

        [XmlElement("smalliconpath")]
        public string SmallIconPath { get; set; }
    }

    public class IconSetting
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

    public class WorldSetting
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("shortname")]
        public string ShortName { get; set; }

        [XmlElement("alias")]
        public string AliasShortName { get; set; }

        [XmlElement("conversion")]
        public ConversionSetting conversionsetting { get; set; }

        [XmlElement("tilesource")]
        public string filename { get; set; }
    }

    public class ConversionSetting
    {
        [XmlAttribute("slope")]
        public double Slope { get; set; }

        [XmlAttribute("xi")]
        public double xintercept { get; set; }

        [XmlAttribute("yi")]
        public double yintercept { get; set; }
    }


}
