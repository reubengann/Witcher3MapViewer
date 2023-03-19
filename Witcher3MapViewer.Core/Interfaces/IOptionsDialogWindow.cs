namespace Witcher3MapViewer.Core.Interfaces
{
    public class PolicyOptions
    {
        public bool ShowRaces { get; set; }


    }
    public interface IOptionsDialogWindow
    {
        PolicyOptions ShowDialog();
    }
}
