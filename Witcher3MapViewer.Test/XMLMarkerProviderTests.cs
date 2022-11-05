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
    }
}
