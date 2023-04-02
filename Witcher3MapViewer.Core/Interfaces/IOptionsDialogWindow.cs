﻿namespace Witcher3MapViewer.Core.Interfaces
{

    public interface IOptionsDialogWindow
    {
        bool ShowDialog();
        bool ResetWasRequested();
        bool ModeChanged();
        Options GetNewOptions();
    }
}
