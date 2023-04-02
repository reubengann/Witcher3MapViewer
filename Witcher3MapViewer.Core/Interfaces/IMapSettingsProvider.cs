namespace Witcher3MapViewer.Core
{
    public interface IMapSettingsProvider
    {
        WorldSetting GetWorldSetting(string worldShortName);
        IconSettings GetIconSettings();
        List<WorldSetting> GetAll();
    }
}
