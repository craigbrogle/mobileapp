using System;
using System.Reactive.Concurrency;
using MvvmCross.Core.Navigation;
using MvvmCross.Platform;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Autocomplete;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Login;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.Services;
using Toggl.Foundation.Shortcuts;
using Toggl.Foundation.Sync;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave;
using MvvmCross.Core.ViewModels;

namespace Toggl.Foundation.MvvmCross
{
    public sealed class FoundationMvvmCross
    {
        internal ITogglDatabase Database { get; }

        internal IApiFactory ApiFactory { get; }

        internal ITimeService TimeService { get; }

        internal IScheduler Scheduler { get; }

        internal IAnalyticsService AnalyticsService { get; }

        internal IGoogleService GoogleService { get; }

        internal IMvxNavigationService NavigationService { get; }

        internal IApiErrorHandlingService ApiErrorHandlingService { get; }

        internal IOnboardingStorage OnboardingStorage { get; }

        internal IAccessRestrictionStorage AccessRestrictionStorage { get; }

        internal IBackgroundService BackgroundService { get; }

        internal IApplicationShortcutCreator ShortcutCreator { get; }

        internal FoundationMvvmCross(
            IApiFactory apiFactory,
            ITogglDatabase database,
            ITimeService timeService,
            IScheduler scheduler,
            IAnalyticsService analyticsService,
            IGoogleService googleService,
            IApplicationShortcutCreator shortcutCreator,
            IBackgroundService backgroundService,
            IOnboardingStorage onboardingStorage,
            IAccessRestrictionStorage accessRestrictionStorage,
            IMvxNavigationService navigationService,
            IApiErrorHandlingService apiErrorHandlingService)
        {
            Database = database;
            ApiFactory = apiFactory;
            TimeService = timeService;
            Scheduler = scheduler;
            AnalyticsService = analyticsService;
            GoogleService = googleService;
            ShortcutCreator = shortcutCreator;
            BackgroundService = backgroundService;
            OnboardingStorage = onboardingStorage;
            AccessRestrictionStorage = accessRestrictionStorage;
            NavigationService = navigationService;
            ApiErrorHandlingService = apiErrorHandlingService;
        }
    }

    public static class FoundationExtensions
    {
        private const int newUserThreshold = 60;
        private static readonly TimeSpan retryDelayLimit = TimeSpan.FromSeconds(60);

        public static FoundationMvvmCross RegisterServices(this Foundation self,
            IDialogService dialogService,
            IBrowserService browserService,
            IKeyValueStorage keyValueStorage,
            IAccessRestrictionStorage accessRestrictionStorage,
            IUserPreferences userPreferences,
            IOnboardingStorage onboardingStorage,
            IMvxNavigationService navigationService,
            IPasswordManagerService passwordManagerService = null)
        {
            Ensure.Argument.IsNotNull(self, nameof(self));
            Ensure.Argument.IsNotNull(dialogService, nameof(dialogService));
            Ensure.Argument.IsNotNull(browserService, nameof(browserService));
            Ensure.Argument.IsNotNull(keyValueStorage, nameof(keyValueStorage));
            Ensure.Argument.IsNotNull(accessRestrictionStorage, nameof(accessRestrictionStorage));
            Ensure.Argument.IsNotNull(userPreferences, nameof(userPreferences));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            var timeService = self.TimeService;

            var apiErrorHandlingService = new ApiErrorHandlingService(navigationService, accessRestrictionStorage);

            Mvx.RegisterSingleton(self.BackgroundService);
            Mvx.RegisterSingleton(dialogService);
            Mvx.RegisterSingleton(self.Database);
            Mvx.RegisterSingleton(browserService);
            Mvx.RegisterSingleton(self.UserAgent);
            Mvx.RegisterSingleton(self.Scheduler);
            Mvx.RegisterSingleton(self.ApiFactory);
            Mvx.RegisterSingleton(self.TimeService);
            Mvx.RegisterSingleton(self.MailService);
            Mvx.RegisterSingleton(self.ShortcutCreator);
            Mvx.RegisterSingleton(self.LicenseProvider);
            Mvx.RegisterSingleton(self.ShortcutCreator);
            Mvx.RegisterSingleton(self.AnalyticsService);
            Mvx.RegisterSingleton(self.PlatformConstants);
            Mvx.RegisterSingleton(self.Database.IdProvider);
            Mvx.RegisterSingleton(self.SuggestionProviderContainer);
            Mvx.RegisterSingleton(userPreferences);
            Mvx.RegisterSingleton(onboardingStorage);
            Mvx.RegisterSingleton(accessRestrictionStorage);
            Mvx.RegisterSingleton<IApiErrorHandlingService>(apiErrorHandlingService);
            Mvx.RegisterSingleton(passwordManagerService ?? new StubPasswordManagerService());

            return new FoundationMvvmCross(
                self.ApiFactory,
                self.Database,
                timeService,
                self.Scheduler,
                self.AnalyticsService,
                self.GoogleService,
                self.ShortcutCreator,
                self.BackgroundService,
                onboardingStorage,
                accessRestrictionStorage,
                navigationService,
                apiErrorHandlingService);
        }

        public static FoundationMvvmCross RevokeNewUserIfNeeded(this FoundationMvvmCross self)
        {
            var now = self.TimeService.CurrentDateTime;
            var lastUsed = self.OnboardingStorage.GetLastOpened();
            self.OnboardingStorage.SetLastOpened(now);
            if (lastUsed == null) return self;

            var lastUsedDate = DateTimeOffset.Parse(lastUsed);
            var offset = now - lastUsedDate;
            if (offset < TimeSpan.FromDays(newUserThreshold)) return self;

            self.OnboardingStorage.SetIsNewUser(false);
            return self;
        }

        public static void Initialize<T>(this FoundationMvvmCross self, App<T> app, IScheduler scheduler)
            where T : MvxViewModel
        {
            Func<ITogglDataSource, ISyncManager> createSyncManager(ITogglApi api) => dataSource =>
                TogglSyncManager.CreateSyncManager(self.Database, api, dataSource, self.TimeService, self.AnalyticsService, retryDelayLimit, scheduler);

            ITogglDataSource createDataSource(ITogglApi api)
            {
                var dataSource = new TogglDataSource(api, self.Database, self.TimeService, self.ApiErrorHandlingService, self.BackgroundService, createSyncManager(api), TimeSpan.FromMinutes(5), self.ShortcutCreator)
                    .RegisterServices();

                Mvx.ConstructAndRegisterSingleton<IInteractorFactory, InteractorFactory>();
                Mvx.ConstructAndRegisterSingleton<IAutocompleteProvider, AutocompleteProvider>();

                return dataSource;
            }

            var loginManager =
                new LoginManager(self.ApiFactory, self.Database, self.GoogleService, self.ShortcutCreator, self.AccessRestrictionStorage, createDataSource);

            Mvx.RegisterSingleton<ILoginManager>(loginManager);

            app.Initialize(loginManager, self.NavigationService, self.AccessRestrictionStorage);
        }
    }
}
