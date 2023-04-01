﻿namespace Witcher3MapViewer.Core
{
    public interface IQuestAvailabilityProvider
    {
        bool IsQuestAvailable(Quest q);
        void SetState(string guid, QuestStatusState? state);
        QuestStatusState GetState(string guid);
        void ResetManualStates();

        event Action? AvailabilityChanged;
    }
}
