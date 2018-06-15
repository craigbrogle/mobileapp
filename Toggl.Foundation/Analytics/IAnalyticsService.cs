using System;
using System.Collections.Generic;

namespace Toggl.Foundation.Analytics
{
    public interface IAnalyticsService
    {
        IAnalyticsEvent<AuthenticationMethod> Login { get; }

        IAnalyticsEvent<LoginErrorSource> LoginError { get; }

        IAnalyticsEvent<AuthenticationMethod> SignUp { get; }

        IAnalyticsEvent<SignUpErrorSource> SignUpError { get; }

        IAnalyticsEvent<string> OnboardingSkip { get; }

        IAnalyticsEvent<LogoutSource> Logout { get; }

        IAnalyticsEvent ResetPassword { get; }

        IAnalyticsEvent PasswordManagerButtonClicked { get; }

        IAnalyticsEvent PasswordManagerContainsValidEmail { get; }

        IAnalyticsEvent PasswordManagerContainsValidPassword { get; }

        IAnalyticsEvent<Type> CurrentPage { get; }

        IAnalyticsEvent<TimeEntryStartOrigin> TimeEntryStarted { get; }

        IAnalyticsEvent DeleteTimeEntry { get; }

        IAnalyticsEvent<string> ApplicationShortcut { get; }

        IAnalyticsEvent EditEntrySelectProject { get; }

        IAnalyticsEvent EditEntrySelectTag { get; }

        IAnalyticsEvent<ProjectTagSuggestionSource> StartEntrySelectProject { get; }

        IAnalyticsEvent<ProjectTagSuggestionSource> StartEntrySelectTag { get; }

        IAnalyticsEvent<ReportsSource, int, int, double> ReportsSuccess { get; }

        IAnalyticsEvent<ReportsSource, int, double> ReportsFailure { get; }

        IAnalyticsEvent OfflineModeDetected { get; }

        IAnalyticsEvent<int> ProjectGhostsCreated { get; }
      
        IAnalyticsEvent<EditViewTapSource> EditViewTapped { get; }

        IAnalyticsEvent<StartViewTapSource> StartViewTapped { get; }

        void Track(string eventName, Dictionary<string, string> parameters = null);

        void Track(Exception exception);
    }
}
