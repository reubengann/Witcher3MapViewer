using Moq;
using System.Text;
using Witcher3MapViewer.Core;

namespace Witcher3MapViewer.Test
{
    public class XMLMarkerProviderTests
    {
        private const string mockFull = @"<?xml version=""1.0"" ?>
                            <mappins>
                               <world code=""WO"" name=""White Orchard"">
	                            <mappin type=""RoadSign"">
                                     <position x=""101"" y=""-9""/>
                                     <name>Woesong bridge</name>
                                     <internalname>woesong_bridge</internalname>
                                  </mappin>
	                               <mappin type=""PlaceOfPower"">
                                     <position x=""-94"" y=""-324""/>
                                     <name>pop_quen2_prlg</name>
                                     <internalname>pop_quen2_prlg</internalname>
                                  </mappin>
                               </world>
                            </mappins>";
        Mock<IMapSettingsProvider> mockSettingsProvider;

        [SetUp]
        public void SetUp()
        {
            mockSettingsProvider = new Mock<IMapSettingsProvider>();
            mockSettingsProvider.Setup(x => x.GetIconSettings()).Returns(() => new IconSettings()
            {
                LargeIconPath = "MarkerImages",
                IconInfos = new List<IconInfo>() {
                    new IconInfo() { InternalName = "RoadSign", Image = "RoadSign.png" },
                    new IconInfo() { InternalName = "PlaceOfPower", Image = "PlaceOfPower.png" },
                }
            });
            mockSettingsProvider.Setup(x => x.GetWorldSetting("WO")).Returns(() => new WorldSetting()
            {
                ShortName = "WO",
                Slope = 2,
                XIntercept = 10,
                YIntercept = -5
            });
        }

        [Test]
        public void LoadsFileWhenInstantiated()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" ?><mappins></mappins>"));
            var p = new XMLMarkerProvider(ms, mockSettingsProvider.Object);
            Assert.That(ms.Position, Is.EqualTo(ms.Length));
        }

        [Test]
        public void WhenWorldIsPresentThenReturnAMarkerSpec()
        {
            string mock = @"<?xml version=""1.0"" ?>
                            <mappins>
                               <world code=""WO"" name=""White Orchard"">
                               </world>
                            </mappins>";
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mock));
            var p = new XMLMarkerProvider(ms, mockSettingsProvider.Object);
            Assert.DoesNotThrow(() => p.GetMarkerSpecs("WO"));
        }

        [Test]
        public void WhenWorldHasAPinThenItIsInTheSpec()
        {
            string mock = @"<?xml version=""1.0"" ?>
            <mappins>
               <world code=""WO"" name=""White Orchard"">
	            <mappin type=""RoadSign"">
                     <position x=""101"" y=""-9""/>
                     <name>Woesong bridge</name>
                     <internalname>woesong_bridge</internalname>
                  </mappin>
               </world>
            </mappins>";
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mock));
            var p = new XMLMarkerProvider(ms, mockSettingsProvider.Object);
            Assert.That(p.GetMarkerSpecs("WO").Count(), Is.EqualTo(1));
        }

        [Test]
        public void WhenWorldHasTwoPinsThenTheyIsInTheSpec()
        {
            string mock = mockFull;
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mock));
            var p = new XMLMarkerProvider(ms, mockSettingsProvider.Object);
            Assert.That(p.GetMarkerSpecs("WO").Count(), Is.EqualTo(2));
        }

        [Test]
        public void CallsGetIconSettings()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mockFull));
            var p = new XMLMarkerProvider(ms, mockSettingsProvider.Object);
            mockSettingsProvider.Verify(x => x.GetIconSettings());
        }

        [Test]
        public void UsesInternalNameFromMapSettingsToLookupImage()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mockFull));
            var p = new XMLMarkerProvider(ms, mockSettingsProvider.Object);
            var result = p.GetMarkerSpecs("WO");
            Assert.That(result[0].ImagePath, Is.EqualTo("MarkerImages\\RoadSign.png"));
        }

        [Test]
        public void WhenMakingSpecConvertsWorldPositionUsingFormula()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mockFull));
            var p = new XMLMarkerProvider(ms, mockSettingsProvider.Object);
            var result = p.GetMarkerSpecs("WO");
            double expectedX = 101 * 2 + 10;
            double expectedY = -9 * 2 - 5;
            Assert.That(result[0].WorldLocations[0].X, Is.EqualTo(expectedX).Within(0.00001));
            Assert.That(result[0].WorldLocations[0].Y, Is.EqualTo(expectedY).Within(0.00001));
        }

        [Test]
        public void WhenPinUsesAliasLooksUpCorrectImage()
        {
            string mock = @"<?xml version=""1.0"" ?>
                            <mappins>
                               <world code=""WO"" name=""White Orchard"">
	                            <mappin type=""RoadSignAlias"">
                                     <position x=""101"" y=""-9""/>
                                     <name>Woesong bridge</name>
                                     <internalname>woesong_bridge</internalname>
                                  </mappin>
                               </world>
                            </mappins>";
            mockSettingsProvider.Setup(x => x.GetIconSettings()).Returns(() => new IconSettings()
            {
                LargeIconPath = "MarkerImages",
                IconInfos = new List<IconInfo>() {
                    new IconInfo() { InternalName = "RoadSign", Image = "RoadSign.png", Aliases = new List<string> { "RoadSignAlias" } },
                }
            });
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mock));
            var p = new XMLMarkerProvider(ms, mockSettingsProvider.Object);
            var result = p.GetMarkerSpecs("WO");
            Assert.That(result[0].ImagePath, Is.EqualTo("MarkerImages\\RoadSign.png"));
        }
    }
}
