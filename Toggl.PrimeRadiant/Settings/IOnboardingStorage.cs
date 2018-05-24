using System;
using Toggl.PrimeRadiant.Onboarding;

namespace Toggl.PrimeRadiant.Settings
{
    public interface IOnboardingStorage
    {
        IObservable<bool> IsNewUser { get; }
        IObservable<bool> UserSignedUpUsingTheApp { get; }
        IObservable<bool> StartButtonWasTappedBefore { get; }
        IObservable<bool> HasTappedTimeEntry { get; }
        IObservable<bool> HasEditedTimeEntry { get; }
        IObservable<bool> StopButtonWasTappedBefore { get; }
        IObservable<bool> HasSelectedProject { get; }
        IObservable<bool> ProjectOrTagWasAddedBefore { get; }

        void SetCompletedOnboarding();
        void SetIsNewUser(bool isNewUser);
        void SetLastOpened(DateTimeOffset dateString);
        void SetUserSignedUp();

        string GetLastOpened();
        bool CompletedOnboarding();

        void StartButtonWasTapped();
        void TimeEntryWasTapped();
        void ProjectOrTagWasAdded();
        void StopButtonWasTapped();

        void EditedTimeEntry();
        void SelectsProject();

        bool WasDismissed(IDismissable dismissable);
        void Dismiss(IDismissable dismissable);

        void Reset();
    }
}
