namespace Witcher3MapViewer.Core
{
    public interface IMarkerProvider
    {
        List<MarkerSpec> GetMarkerSpecs(string worldName);
    }
}
