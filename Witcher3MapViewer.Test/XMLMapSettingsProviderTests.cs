using System.Text;
using Witcher3MapViewer.Core;

namespace Witcher3MapViewer.Test
{
    public class XMLMapSettingsProviderTests
    {
        [Test]
        public void WhenInstantiatedReadsFile()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" ?><settings></settings>"));
            var p = new XMLMapSettingsProvider(ms);
            Assert.That(ms.Position, Is.EqualTo(ms.Length));
        }

        [Test]
        public void WhenHasWorldSettingGetsIt()
        {
            string mock = @"<settings>
	                        <worldsettings>
		                        <worldsetting name=""White Orchard"" shortname=""WO"">
			                        <conversion slope=""0.000038"" xi=""60"" yi=""19.2""/>
			                        <tilesource>Maps\WhiteOrchard.mbtiles</tilesource>
		                        </worldsetting>
	                        </worldsettings>
                        </settings>	";
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mock));
            var p = new XMLMapSettingsProvider(ms);
            Assert.DoesNotThrow(() => p.GetWorldSetting("WO"));
        }
        [Test]
        public void WhenHasWorldSettingParsesConversion()
        {
            string mock = @"<settings>
	                        <worldsettings>
		                        <worldsetting name=""White Orchard"" shortname=""WO"">
			                        <conversion slope=""0.000038"" xi=""60"" yi=""19.2""/>
			                        <tilesource>Maps\WhiteOrchard.mbtiles</tilesource>
		                        </worldsetting>
	                        </worldsettings>
                        </settings>	";
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mock));
            var p = new XMLMapSettingsProvider(ms);
            WorldSetting ws = p.GetWorldSetting("WO");
            Assert.That(ws.YIntercept, Is.EqualTo(19.2).Within(0.000001));
            Assert.That(ws.XIntercept, Is.EqualTo(60).Within(0.0000001));
            Assert.That(ws.Slope, Is.EqualTo(0.000038).Within(0.0000001));
            Assert.That(ws.TileSource, Is.EqualTo("Maps\\WhiteOrchard.mbtiles"));
        }

        [Test]
        public void WhenHasIconSettingsLoadsSomething()
        {
            string mock = @"<settings>
	                            <iconsettings>
		                            <largeiconpath>MarkerImages</largeiconpath>
		                            <smalliconpath>SmallMarkerImages</smalliconpath>
	                            </iconsettings>
                            </settings>	";
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mock));
            var p = new XMLMapSettingsProvider(ms);
            Assert.That(p.GetIconSettings(), Is.Not.Null);
        }

        [Test]
        public void WhenHasIconSettingsHasPathsLoadsThem()
        {
            string mock = @"<settings>
	                            <iconsettings>
		                            <largeiconpath>MarkerImages</largeiconpath>
		                            <smalliconpath>SmallMarkerImages</smalliconpath>
	                            </iconsettings>
                            </settings>	";
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mock));
            var p = new XMLMapSettingsProvider(ms);
            var result = p.GetIconSettings();
            Assert.That(result.SmallIconPath, Is.EqualTo("SmallMarkerImages"));
            Assert.That(result.LargeIconPath, Is.EqualTo("MarkerImages"));
        }

        [Test]
        public void WhenHasIconInfosLoadsThem()
        {
            string mock = @"<settings>
	                            <iconsettings>
		                            <largeiconpath>MarkerImages</largeiconpath>
		                            <smalliconpath>SmallMarkerImages</smalliconpath>
		                            <icon>
			                            <image>NoticeBoard.png</image>
			                            <internalname>NoticeBoard</internalname>
			                            <groupname>Notice boards</groupname>
		                            </icon>
		                            <icon>
			                            <image>MonsterNest.png</image>
			                            <internalname>MonsterNest</internalname>
			                            <groupname>Monster nests</groupname>
		                            </icon>
	                            </iconsettings>
                            </settings>	";
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mock));
            var p = new XMLMapSettingsProvider(ms);
            var result = p.GetIconSettings();
            Assert.That(result.IconInfos.Count, Is.EqualTo(2));
        }
    }
}
