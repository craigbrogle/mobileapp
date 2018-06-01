using System;

namespace Toggl.PrimeRadiant.Settings
{
    public interface IUserPreferences
    {
        IObservable<bool> IsManualModeEnabledObservable { get; }

        bool IsManualModeEnabled { get; }

        void EnableManualMode();

        void EnableTimerMode();

        void Reset();
    }
}