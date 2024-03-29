﻿using System.Xml.Serialization;
using Witcher3MapViewer.Core.DAO;

namespace Witcher3MapViewer.Core
{
    public class XMLMarkerProvider : IMarkerProvider
    {
        private Dictionary<string, List<MarkerSpec>> Items;
        IconSettings iconSettings;
        Dictionary<string, string> filenameLookup;
        Dictionary<string, string> nameLookup;
        private Dictionary<string, string> canonicalNameLookup;
        private readonly IMapSettingsProvider mapSettingsProvider;

        public XMLMarkerProvider(Stream s, IMapSettingsProvider mapSettingsProvider)
        {
            this.mapSettingsProvider = mapSettingsProvider;
            Items = new Dictionary<string, List<MarkerSpec>>();
            XmlSerializer serializer = new XmlSerializer(typeof(MapPinCollectionDAO));
            MapPinCollectionDAO? readitems = (MapPinCollectionDAO?)serializer.Deserialize(s);
            if (readitems == null) throw new Exception();
            iconSettings = mapSettingsProvider.GetIconSettings();
            filenameLookup = new Dictionary<string, string>();
            nameLookup = new Dictionary<string, string>();
            canonicalNameLookup = new Dictionary<string, string>();
            foreach (IconInfo? iconInfo in iconSettings.IconInfos)
            {
                filenameLookup[iconInfo.InternalName] = iconInfo.Image;
                nameLookup[iconInfo.InternalName] = iconInfo.GroupName;
                canonicalNameLookup[iconInfo.InternalName] = iconInfo.InternalName;
                if (iconInfo.Aliases != null)
                {
                    foreach (string alias in iconInfo.Aliases)
                    {
                        filenameLookup[alias] = iconInfo.Image;
                        nameLookup[alias] = iconInfo.GroupName;
                        canonicalNameLookup[alias] = iconInfo.InternalName;
                    }

                }
            }
            foreach (MapPinWorldDAO worldDAO in readitems.Worlds)
            {
                Items[worldDAO.Code] = GroupMarkerTypes(worldDAO, worldDAO.Code);
            }

        }

        private List<MarkerSpec> GroupMarkerTypes(MapPinWorldDAO worldDAO, string worldShortName)
        {
            //type maps to InternalName or Alias
            ILookup<string, MapPinDAO>? groups = worldDAO.Pins.ToLookup(x => canonicalNameLookup[x.Type]);
            return groups.Select(x => new MarkerSpec(
                FindPathToIcon(x.Key),
                x.Select(p => ComputeMapPosition(p.Position, worldShortName)).ToList(),
                x.Key,
                LookUpGroupName(x.Key)
                )
            ).ToList();
        }

        private string LookUpGroupName(string key)
        {
            return nameLookup[key];
        }

        private string FindPathToIcon(string key)
        {
            string iconFilename = filenameLookup[key];
            return Path.Combine(iconSettings.LargeIconPath, iconFilename);
        }

        private Point ComputeMapPosition(MapPinPositionDAO worldPosition, string worldShortName)
        {
            WorldSetting worldSetting = mapSettingsProvider.GetWorldSetting(worldShortName);
            return RealToGameSpaceConversion.ToWorldSpace(new Point(worldPosition.x, worldPosition.y), worldSetting);
        }

        public List<MarkerSpec> GetMarkerSpecs(string worldName)
        {
            return Items[worldName];
        }

        public static XMLMarkerProvider FromFile(string filepath, IMapSettingsProvider mapSettingsProvider)
        {
            using FileStream sr = new FileStream(filepath, FileMode.Open);
            return new XMLMarkerProvider(sr, mapSettingsProvider);
        }
    }


}
