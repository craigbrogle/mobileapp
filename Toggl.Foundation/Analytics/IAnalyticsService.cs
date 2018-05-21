using System;
using Toggl.Foundation.Shortcuts;

namespace Toggl.Foundation.Analytics
{
    public interface IAnalyticsService
    {
        void TrackOnboardingSkipEvent(string pageName);

        void TrackLoginEvent(AuthenticationMethod authenticationMethod);
        void TrackLoginErrorEvent(LoginErrorSource source);

        void TrackSignUpEvent(AuthenticationMethod authenticationMethod);
        void TrackSignUpErrorEvent(SignUpErrorSource source);

        void TrackLogoutEvent(LogoutSource source);
        void TrackResetPassword();

        void TrackPasswordManagerButtonClicked();
        void TrackPasswordManagerContainsValidEmail();
        void TrackPasswordManagerContainsValidPassword();

        void TrackCurrentPage(Type viewModelType);

        void TrackNonFatalException(Exception ex);

        void TrackStartedTimeEntry(TimeEntryStartOrigin origin);

        void TrackSyncError(Exception exception);

        void TrackAppShortcut(string shortcut);

        void TrackEditOpensProjectSelector();
        void TrackEditOpensTagSelector();

        void TrackStartOpensProjectSelector(ProjectTagSuggestionSource source);
        void TrackStartOpensTagSelector(ProjectTagSuggestionSource source);
    }
}
