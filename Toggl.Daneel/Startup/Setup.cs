using System;
using System.Reactive.Concurrency;
using Foundation;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.iOS.Platform;
using MvvmCross.iOS.Views.Presenters;
using MvvmCross.Platform;
using MvvmCross.Platform.Platform;
using MvvmCross.Platform.Plugins;
using Toggl.Daneel.Presentation;
using Toggl.Daneel.Services;
using Toggl.Foundation;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Login;
using Toggl.Foundation.MvvmCross;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Services;
using Toggl.Foundation.Suggestions;
using Toggl.PrimeRadiant.Realm;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave;
using Toggl.Ultrawave.Network;
using UIKit;

namespace Toggl.Daneel
{
    public partial class Setup : MvxIosSetup
    {
        private const int maxNumberOfSuggestions = 3;

        private IAnalyticsService analyticsService;
        private IMvxNavigationService navigationService;

#if USE_PRODUCTION_API
        private const ApiEnvironment environment = ApiEnvironment.Production;
#else
        private const ApiEnvironment environment = ApiEnvironment.Staging;
#endif

        public Setup(MvxApplicationDelegate applicationDelegate, UIWindow window)
            : this(applicationDelegate, new TogglPresenter(applicationDelegate, window))
        {
        }

        private Setup(MvxApplicationDelegate applicationDelegate, IMvxIosViewPresenter presenter)
            : base(applicationDelegate, presenter)
        {
        }

        protected override IMvxTrace CreateDebugTrace() => new DebugTrace();

        protected override IMvxApplication CreateApp() => new App<OnboardingViewModel>();

        protected override IMvxNavigationService InitializeNavigationService(IMvxViewModelLocatorCollection collection)
        {
            analyticsService = new AnalyticsService();

            var loader = CreateViewModelLoader(collection);
            Mvx.RegisterSingleton<IMvxViewModelLoader>(loader);

            navigationService = new TrackingNavigationService(null, loader, analyticsService);

            Mvx.RegisterSingleton<IMvxNavigationService>(navigationService);
            return navigationService;
        }

        protected override void InitializeApp(IMvxPluginManager pluginManager, IMvxApplication app)
        {
#if !USE_PRODUCTION_API
            System.Net.ServicePointManager.ServerCertificateValidationCallback
                  += (sender, certificate, chain, sslPolicyErrors) => true;
#endif

            const string clientName = "Daneel";
            var version = NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();
            var database = new Database();
            var scheduler = Scheduler.Default;
            var timeService = new TimeService(scheduler);
            var suggestionProviderContainer = new SuggestionProviderContainer(
                new MostUsedTimeEntrySuggestionProvider(database, timeService, maxNumberOfSuggestions)
            );

            var appVersion = Version.Parse(version);
            var userAgent = new UserAgent(clientName, version);
            var keyValueStorage = new UserDefaultsStorage();
            var settingsStorage = new SettingsStorage(Version.Parse(version), keyValueStorage);

            var foundation =
                TogglFoundation
                    .ForClient(userAgent, appVersion)
                    .WithDatabase(database)
                    .WithScheduler(scheduler)
                    .WithTimeService(timeService)
                    .WithApiEnvironment(environment)
                    .WithGoogleService<GoogleService>()
                    .WithLicenseProvider<LicenseProvider>()
                    .WithAnalyticsService(analyticsService)
                    .WithPlatformConstants<PlatformConstants>()
                    .WithApiFactory(new ApiFactory(environment, userAgent))
                    .WithBackgroundService(new BackgroundService(timeService))
                    .WithApplicationShortcutCreator<ApplicationShortcutCreator>()
                    .WithSuggestionProviderContainer(suggestionProviderContainer)
                    .WithMailService(new MailService((ITopViewControllerProvider)Presenter))

                    .StartRegisteringPlatformServices()
                    .WithBrowserService<BrowserService>()
                    .WithKeyValueStorage(keyValueStorage)
                    .WithUserPreferences(settingsStorage)
                    .WithOnboardingStorage(settingsStorage)
                    .WithNavigationService(navigationService)
                    .WithAccessRestrictionStorage(settingsStorage)
                    .WithPasswordManagerService<OnePasswordService>()
                    .WithDialogService(new DialogService((ITopViewControllerProvider)Presenter))
                    .WithErrorHandlingService(new ErrorHandlingService(navigationService, settingsStorage))
                    .Build();

            foundation.RevokeNewUserIfNeeded().Initialize();

            base.InitializeApp(pluginManager, app);
        }
    }
}
