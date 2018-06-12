using System;
using System.Collections.Generic;
using Toggl.Multivac;

namespace Toggl.Foundation.Analytics
{
    [Preserve(AllMembers = true)]
    public abstract class BaseAnalyticsService : AnalyticsEventAttributeInitializer, IAnalyticsService
    {
        protected BaseAnalyticsService()
        {
            InitializeAttributedProperties(this);
        }

        [AnalyticsEvent("AuthenticationMethod")]
        public IAnalyticsEvent<AuthenticationMethod> Login { get; protected set; }

        [AnalyticsEvent("Source")]
        public IAnalyticsEvent<LoginErrorSource> LoginError { get; protected set; }

        [AnalyticsEvent("AuthenticationMethod")]
        public IAnalyticsEvent<AuthenticationMethod> SignUp { get; protected set; }

        [AnalyticsEvent("Source")]
        public IAnalyticsEvent<SignUpErrorSource> SignUpError { get; protected set; }

        [AnalyticsEvent("PageWhenSkipWasClicked")]
        public IAnalyticsEvent<string> OnboardingSkip { get; protected set; }

        [AnalyticsEvent("Source")]
        public IAnalyticsEvent<LogoutSource> Logout { get; protected set; }

        [AnalyticsEvent]
        public IAnalyticsEvent ResetPassword { get; protected set; }

        [AnalyticsEvent]
        public IAnalyticsEvent PasswordManagerButtonClicked { get; protected set; }

        [AnalyticsEvent]
        public IAnalyticsEvent PasswordManagerContainsValidEmail { get; protected set; }

        [AnalyticsEvent]
        public IAnalyticsEvent PasswordManagerContainsValidPassword { get; protected set; }

        [AnalyticsEvent("CurrentPage")]
        public IAnalyticsEvent<Type> CurrentPage { get; protected set; }

        [AnalyticsEvent("Origin")]
        public IAnalyticsEvent<TimeEntryStartOrigin> TimeEntryStarted { get; protected set; }

        [AnalyticsEvent]
        public IAnalyticsEvent DeleteTimeEntry { get; protected set; }

        [AnalyticsEvent("ApplicationShortcutType")]
        public IAnalyticsEvent<string> ApplicationShortcut { get; protected set; }

        [AnalyticsEvent]
        public IAnalyticsEvent EditEntrySelectProject { get; protected set; }

        [AnalyticsEvent]
        public IAnalyticsEvent EditEntrySelectTag { get; protected set; }

        [AnalyticsEvent("Source")]
        public IAnalyticsEvent<ProjectTagSuggestionSource> StartEntrySelectProject { get; protected set; }

        [AnalyticsEvent("Source")]
        public IAnalyticsEvent<ProjectTagSuggestionSource> StartEntrySelectTag { get; protected set; }

        [AnalyticsEvent]
        public IAnalyticsEvent AppWasRated { get; protected set; }
      
        [AnalyticsEvent]
        public IAnalyticsEvent RatingViewWasShown { get; }
      
        [AnalyticsEvent("isPositive")]
        public IAnalyticsEvent<bool> UserFinishedRatingViewFirstStep { get; }
      
        [AnalyticsEvent("outcome")]
        public IAnalyticsEvent<RatingViewSecondStepOutcome> UserFinishedRatingViewSecondStep { get; }

        [AnalyticsEvent("Source", "TotalDays", "ProjectsNotSynced", "LoadingTime")]
        public IAnalyticsEvent<ReportsSource, int, int, double> ReportsSuccess { get; protected set; }

        [AnalyticsEvent("Source", "TotalDays", "LoadingTime")]
        public IAnalyticsEvent<ReportsSource, int, double> ReportsFailure { get; protected set; }

        [AnalyticsEvent]
        public IAnalyticsEvent OfflineModeDetected { get; protected set; }

        [AnalyticsEvent("NumberOfCreatedGhosts")]
        public IAnalyticsEvent<int> ProjectGhostsCreated { get; protected set; }

        public abstract void Track(string eventName, Dictionary<string, string> parameters = null);

        public abstract void Track(Exception exception);
    }
}
