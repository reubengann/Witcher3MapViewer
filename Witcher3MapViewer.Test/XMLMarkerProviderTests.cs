using System.Text;
using Witcher3MapViewer.Core;

namespace Witcher3MapViewer.Test
{
    public class XMLMarkerProviderTests
    {
        [Test]
        public void LoadsFileWhenInstantiated()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" ?><mappins></mappins>"));
            var p = new XMLMarkerProvider(ms);
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
            var p = new XMLMarkerProvider(ms);
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
            var p = new XMLMarkerProvider(ms);
            Assert.That(p.GetMarkerSpecs("WO").Count(), Is.EqualTo(1));
        }

        [Test]
        public void WhenWorldHasTwoPinsThenTheyIsInTheSpec()
        {
            string mock = @"<?xml version=""1.0"" ?>
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
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(mock));
            var p = new XMLMarkerProvider(ms);
            Assert.That(p.GetMarkerSpecs("WO").Count(), Is.EqualTo(2));
        }

    }
}
