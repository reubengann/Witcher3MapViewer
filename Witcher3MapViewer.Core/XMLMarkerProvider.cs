namespace Witcher3MapViewer.Core
{
    public class XMLMarkerProvider : IMarkerProvider
    {
        public List<MarkerSpec> GetMarkerSpecs(string worldName)
        {
            return new List<MarkerSpec> { MapMarkers.MapMarkerSpec };
        }
    }
}
