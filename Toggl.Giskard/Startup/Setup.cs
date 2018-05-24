using System;
using System.Reactive.Concurrency;
using Android.Content;
using MvvmCross.Binding;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views;
using MvvmCross.Platform;
using MvvmCross.Platform.Platform;
using MvvmCross.Platform.Plugins;
using Toggl.Foundation;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.MvvmCross;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Suggestions;
using Toggl.Giskard.Presenters;
using Toggl.Giskard.Services;
using Toggl.PrimeRadiant.Realm;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave;

namespace Toggl.Giskard
{
    public sealed partial class Setup : MvxAppCompatSetup
    {
        private const int maxNumberOfSuggestions = 5;

        private IAnalyticsService analyticsService;
        private IMvxNavigationService navigationService;

#if USE_PRODUCTION_API
        private const ApiEnvironment environment = ApiEnvironment.Production;
#else
        private const ApiEnvironment environment = ApiEnvironment.Staging;
#endif

        public Setup(Context applicationContext) 
            : base(applicationContext)
        {
        }

        protected override IMvxApplication CreateApp() => new App<LoginViewModel>();

        protected override IMvxTrace CreateDebugTrace() => new DebugTrace();

        protected override MvxBindingBuilder CreateBindingBuilder() => new TogglBindingBuilder();

        protected override IMvxNavigationService InitializeNavigationService(IMvxViewModelLocatorCollection collection)
        {
            analyticsService = new AnalyticsService();

            var loader = CreateViewModelLoader(collection);
            Mvx.RegisterSingleton<IMvxViewModelLoader>(loader);

            navigationService = new TrackingNavigationService(null, loader, analyticsService);

            Mvx.RegisterSingleton<IMvxNavigationService>(navigationService);
            return navigationService;
        }

        protected override IMvxAndroidViewPresenter CreateViewPresenter() 
            => new TogglPresenter(AndroidViewAssemblies);

        protected override void InitializeApp(IMvxPluginManager pluginManager, IMvxApplication app)
        {
            base.InitializeApp(pluginManager, app);

            const string clientName = "Giskard";
            var packageInfo = ApplicationContext.PackageManager.GetPackageInfo(ApplicationContext.PackageName, 0);
            var version = packageInfo.VersionName;
            var sharedPreferences = ApplicationContext.GetSharedPreferences(clientName, FileCreationMode.Private);
            var database = new Database();
            var scheduler = Scheduler.Default;
            var timeService = new TimeService(scheduler);
            var suggestionProviderContainer = new SuggestionProviderContainer(
                new MostUsedTimeEntrySuggestionProvider(database, timeService, maxNumberOfSuggestions)
            );

            var keyValueStorage = new SharedPreferencesStorage(sharedPreferences);
            var settingsStorage = new SettingsStorage(Version.Parse(version), keyValueStorage);

            var foundation = Foundation.Foundation.Create(
                clientName,
                version,
                database,
                timeService,
                scheduler,
                new MailService(ApplicationContext),
                new GoogleService(),
                environment,
                new LicenseProvider(),
                analyticsService,
                new PlatformConstants(),
                new ApplicationShortcutCreator(ApplicationContext),
                suggestionProviderContainer
            );

            foundation
                .RegisterServices(
                    new DialogService(),
                    new BrowserService(), 
                    keyValueStorage,
                    settingsStorage,
                    settingsStorage,
                    settingsStorage,
                    navigationService,
                    new OnePasswordService())
               .RevokeNewUserIfNeeded()
               .Initialize(app as App<LoginViewModel>, Scheduler.Default);
        }
    }
}
