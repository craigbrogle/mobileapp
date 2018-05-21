using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.MvvmCross.ViewModels.Settings;
using Toggl.Foundation.Services;
using Toggl.Foundation.Sync;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave.Network;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SettingsViewModel : MvxViewModel
    {
        private const string feedbackRecipient = "support@toggl.com";
        private readonly CompositeDisposable disposeBag = new CompositeDisposable();

        private readonly ITogglDataSource dataSource;
        private readonly IDialogService dialogService;
        private readonly IPlatformConstants platformConstants;
        private readonly IInteractorFactory interactorFactory;
        private readonly IMvxNavigationService navigationService;
        private readonly IUserPreferences userPreferences;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IMailService mailService;
        private readonly UserAgent userAgent;
        private readonly IAnalyticsService analyticsService;

        private long workspaceId;

        public string Title { get; private set; } = Resources.Settings;

        public Email Email { get; private set; }

        public string Name { get; private set; } = "";

        public string Version { get; private set; } = "";

        public string WorkspaceName { get; private set; } = "";

        public string CurrentPlan { get; private set; } = "";

        public bool UseTwentyFourHourClock { get; set; }

        public bool AddMobileTag { get; set; }

        public bool IsManualModeEnabled { get; set; }

        public bool IsLoggingOut { get; private set; }

        public bool IsRunningSync { get; private set; }

        public bool IsSynced { get; private set; }

        public DateFormat DateFormat { get; private set; }

        public DurationFormat DurationFormat { get; private set; }

        public BeginningOfWeek BeginningOfWeek { get; private set; }

        public IMvxCommand RateCommand { get; }

        public IMvxCommand HelpCommand { get; }

        public IMvxCommand UpdateCommand { get; }

        public IMvxAsyncCommand BackCommand { get; }

        public IMvxAsyncCommand LogoutCommand { get; }

        public IMvxCommand EditProfileCommand { get; }

        public IMvxAsyncCommand SubmitFeedbackCommand { get; }

        public IMvxAsyncCommand AboutCommand { get; }

        public IMvxCommand EditSubscriptionCommand { get; }

        public IMvxAsyncCommand PickWorkspaceCommand { get; }

        public IMvxAsyncCommand SelectDateFormatCommand { get; }

        public IMvxAsyncCommand SelectDurationFormatCommand { get; }

        public IMvxAsyncCommand SelectBeginningOfWeekCommand { get; }

        public IMvxCommand ToggleAddMobileTagCommand { get; }

        public IMvxAsyncCommand ToggleUseTwentyFourHourClockCommand { get; }

        public IMvxCommand ToggleManualModeCommand { get; }

        public IMvxAsyncCommand<SelectableWorkspaceViewModel> SelectDefaultWorkspaceCommand { get; }

        public MvxObservableCollection<SelectableWorkspaceViewModel> Workspaces { get; }
            = new MvxObservableCollection<SelectableWorkspaceViewModel>();

        public SettingsViewModel(
            UserAgent userAgent,
            IMailService mailService,
            ITogglDataSource dataSource,
            IDialogService dialogService,
            IInteractorFactory interactorFactory,
            IPlatformConstants platformConstants,
            IUserPreferences userPreferences,
            IOnboardingStorage onboardingStorage,
            IMvxNavigationService navigationService,
            IAnalyticsService analyticsService)
        {
            Ensure.Argument.IsNotNull(userAgent, nameof(userAgent));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(mailService, nameof(mailService));
            Ensure.Argument.IsNotNull(dialogService, nameof(dialogService));
            Ensure.Argument.IsNotNull(userPreferences, nameof(userPreferences));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(platformConstants, nameof(platformConstants));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.userAgent = userAgent;
            this.dataSource = dataSource;
            this.mailService = mailService;
            this.dialogService = dialogService;
            this.interactorFactory = interactorFactory;
            this.navigationService = navigationService;
            this.platformConstants = platformConstants;
            this.userPreferences = userPreferences;
            this.onboardingStorage = onboardingStorage;
            this.analyticsService = analyticsService;

            disposeBag.Add(dataSource.SyncManager
                .ProgressObservable
                .Subscribe(async progress =>
                {
                    IsRunningSync = IsLoggingOut == false && progress == SyncProgress.Syncing;
                    IsSynced = IsLoggingOut == false && progress == SyncProgress.Synced && await isSynced();
                })
            );

            RateCommand = new MvxCommand(rate);
            HelpCommand = new MvxCommand(help);
            UpdateCommand = new MvxCommand(update);
            BackCommand = new MvxAsyncCommand(back);
            LogoutCommand = new MvxAsyncCommand(maybeLogout);
            EditProfileCommand = new MvxCommand(editProfile);
            EditSubscriptionCommand = new MvxCommand(editSubscription);
            ToggleManualModeCommand = new MvxCommand(toggleManualMode);
            SubmitFeedbackCommand = new MvxAsyncCommand(submitFeedback);
            AboutCommand = new MvxAsyncCommand(openAboutPage);
            ToggleAddMobileTagCommand = new MvxCommand(toggleAddMobileTag);
            SelectDateFormatCommand = new MvxAsyncCommand(selectDateFormat);
            SelectDurationFormatCommand = new MvxAsyncCommand(selectDurationFormat);
            SelectBeginningOfWeekCommand = new MvxAsyncCommand(selectBeginningOfWeek);
            PickWorkspaceCommand = new MvxAsyncCommand(pickDefaultWorkspace);
            ToggleUseTwentyFourHourClockCommand = new MvxAsyncCommand(toggleUseTwentyFourHourClock);
            SelectDefaultWorkspaceCommand = new MvxAsyncCommand<SelectableWorkspaceViewModel>(selectDefaultWorkspace);
        }

        public override async Task Initialize()
        {
            var user = await dataSource.User.Current;
            var defaultWorkspace = await interactorFactory.GetDefaultWorkspace().Execute();

            Email = user.Email;
            Name = user.Fullname;
            Version = userAgent.Version;
            workspaceId = defaultWorkspace.Id;
            WorkspaceName = defaultWorkspace.Name;
            IsManualModeEnabled = userPreferences.IsManualModeEnabled();
            BeginningOfWeek = user.BeginningOfWeek;

            var workspaces = await interactorFactory.GetAllWorkspaces().Execute();
            foreach (var workspace in workspaces)
            {
                Workspaces.Add(new SelectableWorkspaceViewModel(workspace, workspace.Id == workspaceId));
            }

            dataSource.Preferences.Current
                .Subscribe(updateFromPreferences);
        }

        private void updateFromPreferences(IDatabasePreferences preferences)
        {
            DateFormat = preferences.DateFormat;
            DurationFormat = preferences.DurationFormat;
            UseTwentyFourHourClock = preferences.TimeOfDayFormat.IsTwentyFourHoursFormat;
        }

        public void rate() => throw new NotImplementedException();

        public void help() => navigationService
            .Navigate<BrowserViewModel, BrowserParameters>(
                BrowserParameters.WithUrlAndTitle(platformConstants.HelpUrl, Resources.Help));

        public void update() => throw new NotImplementedException();

        public void editProfile() 
        {
        }

        public async Task pickDefaultWorkspace()
        {
            var parameters = WorkspaceParameters.Create(workspaceId, Resources.SetDefaultWorkspace, allowQuerying: false);
            var selectedWorkspaceId =
                await navigationService
                    .Navigate<SelectWorkspaceViewModel, WorkspaceParameters, long>(parameters);
            
            await changeDefaultWorkspace(selectedWorkspaceId);
        }

        private async Task selectDefaultWorkspace(SelectableWorkspaceViewModel workspace)
        {
            foreach (var ws in Workspaces)
                ws.Selected = ws.WorkspaceId == workspace.WorkspaceId;

            await changeDefaultWorkspace(workspace.WorkspaceId);
        }

        private async Task changeDefaultWorkspace(long selectedWorkspaceId)
        {
            if (selectedWorkspaceId == workspaceId) return;

            var workspace = await interactorFactory.GetWorkspaceById(selectedWorkspaceId).Execute();
            workspaceId = selectedWorkspaceId;
            WorkspaceName = workspace.Name;

            await dataSource.User.UpdateWorkspace(workspaceId);
            dataSource.SyncManager.PushSync();
        }

        private async Task submitFeedback()
        {
            var version = userAgent.ToString();
            var phone = platformConstants.PhoneModel;
            var os = platformConstants.OperatingSystem;

            var messageBuilder = new StringBuilder();
            messageBuilder.Append("\n\n"); // 2 leading newlines, so user user can type something above this info
            messageBuilder.Append($"Version: {version}\n");
            if (phone != null)
            {
                messageBuilder.Append($"Phone: {phone}\n");
            }
            messageBuilder.Append($"OS: {os}");

            var mailResult = await mailService.Send(
                feedbackRecipient,
                platformConstants.FeedbackEmailSubject,
                messageBuilder.ToString()
            );

            if (mailResult.Success || string.IsNullOrEmpty(mailResult.ErrorTitle))
                return;

            await dialogService.Alert(
                mailResult.ErrorTitle,
                mailResult.ErrorMessage,
                Resources.Ok
            );
        }

        public void editSubscription() => throw new NotImplementedException();

        public void toggleAddMobileTag() => AddMobileTag = !AddMobileTag;

        public async Task toggleUseTwentyFourHourClock()
        {
            UseTwentyFourHourClock = !UseTwentyFourHourClock;
            var timeFormat = UseTwentyFourHourClock
                ? TimeFormat.TwentyFourHoursFormat
                : TimeFormat.TwelveHoursFormat;

            var preferencesDto = new EditPreferencesDTO { TimeOfDayFormat = timeFormat };
            await updatePreferences(preferencesDto);
        }

        private void toggleManualMode()
        {
            IsManualModeEnabled = !IsManualModeEnabled;
        }

        private void OnIsManualModeEnabledChanged()
        {
            if (IsManualModeEnabled)
            {
                userPreferences.EnableManualMode();
            }
            else
            {
                userPreferences.EnableTimerMode();
            }
        }

        private Task back() => navigationService.Close(this);

        private async Task maybeLogout()
        {
            if (await isSynced())
            {
                await logout();
                return;
            }

            var (title, message) = IsRunningSync
                ? (Resources.SettingsSyncInProgressTitle, Resources.SettingsSyncInProgressMessage)
                : (Resources.SettingsUnsyncedTitle, Resources.SettingsUnsyncedMessage);

            var shouldLogout = await dialogService.Confirm(
                title,
                message,
                Resources.SettingsDialogButtonSignOut,
                Resources.Cancel
            );

            if (!shouldLogout) return;

            await logout();
        }

        private async Task logout()
        {
            IsLoggingOut = true;
            IsSynced = false;
            IsRunningSync = false;
            analyticsService.TrackLogoutEvent(LogoutSource.Settings);
            userPreferences.Reset();
            await dataSource.Logout();
            await navigationService.Navigate<OnboardingViewModel>();
        }

        private async Task<bool> isSynced()
            => !IsRunningSync && !(await dataSource.HasUnsyncedData());

        private async Task selectDateFormat()
        {
            var newDateFormat = await navigationService
                .Navigate<SelectDateFormatViewModel, DateFormat, DateFormat>(DateFormat);

            if (DateFormat == newDateFormat)
                return;
            
            var preferencesDto = new EditPreferencesDTO { DateFormat = newDateFormat };
            var newPreferences = await updatePreferences(preferencesDto);
            DateFormat = newPreferences.DateFormat;
        }

        private async Task selectBeginningOfWeek()
        {
            var newBeginningOfWeek = await navigationService
                .Navigate<SelectBeginningOfWeekViewModel, BeginningOfWeek, BeginningOfWeek>(BeginningOfWeek);

            if (BeginningOfWeek == newBeginningOfWeek)
                return;

            var userDto = new EditUserDTO { BeginningOfWeek = newBeginningOfWeek };
            var newUser = await dataSource.User.Update(userDto);
            BeginningOfWeek = newUser.BeginningOfWeek;

            dataSource.SyncManager.PushSync();
        }

        private Task openAboutPage()
            => navigationService.Navigate<AboutViewModel>();
        
        private async Task selectDurationFormat()
        {
            var newDurationFormat = await navigationService
                .Navigate<SelectDurationFormatViewModel, DurationFormat, DurationFormat>(DurationFormat);

            if (DurationFormat == newDurationFormat)
                return;

            var preferencesDto = new EditPreferencesDTO { DurationFormat = newDurationFormat };
            var newPreferences = await updatePreferences(preferencesDto);
            DurationFormat = newPreferences.DurationFormat;
        }

        private async Task<IDatabasePreferences> updatePreferences(EditPreferencesDTO preferencesDto)
        {
            var newPreferences = await dataSource.Preferences.Update(preferencesDto);
            dataSource.SyncManager.PushSync();
            return newPreferences;
        }
    }
}
