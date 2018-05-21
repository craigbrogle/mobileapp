using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels.Settings;
using Toggl.Foundation.Services;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Tests.Generators;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;
using Toggl.PrimeRadiant.Settings;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class SettingsViewModelTests
    {
        public abstract class SettingsViewModelTest : BaseViewModelTests<SettingsViewModel>
        {
            protected ISubject<SyncProgress> ProgressSubject;

            protected override void AdditionalSetup()
            {
                ProgressSubject = new Subject<SyncProgress>();
                var syncManager = Substitute.For<ISyncManager>();
                syncManager.ProgressObservable.Returns(ProgressSubject.AsObservable());
                DataSource.SyncManager.Returns(syncManager);
            }

            protected override SettingsViewModel CreateViewModel()
                => new SettingsViewModel(
                    UserAgent,
                    MailService,
                    DataSource,
                    DialogService,
                    InteractorFactory,
                    PlatformConstants,
                    UserPreferences,
                    OnboardingStorage,
                    NavigationService,
                    AnalyticsService);
        }

        public sealed class TheConstructor : SettingsViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(TenParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useUserAgent,
                bool useDataSource,
                bool useMailService,
                bool useDialogService,
                bool useInteractorFactory,
                bool usePlatformConstants,
                bool useUserPreferences,
                bool useOnboardingStorage,
                bool useNavigationService,
                bool useAnalyticsService
            )
            {
                var userAgent = useUserAgent ? UserAgent : null;
                var dataSource = useDataSource ? DataSource : null;
                var mailService = useMailService ? MailService : null;
                var dialogService = useDialogService ? DialogService : null;
                var userPreferences = useUserPreferences ? UserPreferences : null;
                var onboardingStorage = useOnboardingStorage ? OnboardingStorage : null;
                var navigationService = useNavigationService ? NavigationService : null;
                var platformConstants = usePlatformConstants ? PlatformConstants : null;
                var interactorFactory = useInteractorFactory ? InteractorFactory : null;
                var analyticsService = useAnalyticsService ? AnalyticsService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new SettingsViewModel(
                        userAgent,
                        mailService,
                        dataSource,
                        dialogService,
                        interactorFactory,
                        platformConstants,
                        userPreferences,
                        onboardingStorage,
                        navigationService,
                        analyticsService);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheFlags : SettingsViewModelTest
        {
            [Property]
            public void SetsIsRunningSyncCorrectly(NonEmptyArray<SyncProgress> statuses)
            {
                foreach (var state in statuses.Get)
                {
                    ProgressSubject.OnNext(state);
                    ViewModel.IsRunningSync.Should().Be(state == SyncProgress.Syncing);
                }
            }

            [Property]
            public void SetsIsSyncedCorrectly(NonEmptyArray<SyncProgress> statuses)
            {
                foreach (var state in statuses.Get)
                {
                    if (state == SyncProgress.Unknown)
                        continue;

                    ProgressSubject.OnNext(state);
                    ViewModel.IsSynced.Should().Be(state == SyncProgress.Synced);
                }
            }

            [Property]
            public void DoesNotEverSetBothIsRunningSyncAndIsSyncedBothToTrue(NonEmptyArray<SyncProgress> statuses)
            {
                foreach (var state in statuses.Get)
                {
                    ProgressSubject.OnNext(state);
                    (ViewModel.IsRunningSync && ViewModel.IsSynced).Should().BeFalse();
                }
            }

            [Property]
            public void DoesNotSetTheIsLoggingOutFlagIfTheLogoutCommandIsNotExecuted(
                NonEmptyArray<SyncProgress> statuses)
            {
                foreach (var state in statuses.Get)
                {
                    ProgressSubject.OnNext(state);
                    ViewModel.IsLoggingOut.Should().BeFalse();
                }
            }

            [Property]
            public void DoesNotUnsetTheIsLoggingOutFlagAfterItIsSetNoMatterWhatStatusesAreObserved(
                NonEmptyArray<SyncProgress> statuses)
            {
                DataSource.Logout().Returns(Observable.Never<Unit>());

                ViewModel.LogoutCommand.ExecuteAsync();

                foreach (var state in statuses.Get)
                {
                    ProgressSubject.OnNext(state);
                    ViewModel.IsLoggingOut.Should().BeTrue();
                }
            }

            [Property]
            public void SetsTheIsRunningSyncAndIsSyncedFlagsToFalseAfterTheIsLoggingInFlagIsSetAndDoesNotSetThemToTrueNoMatterWhatStatusesAreObserved(NonEmptyArray<SyncProgress> statuses)
            {
                DataSource.Logout().Returns(Observable.Never<Unit>());

                ViewModel.LogoutCommand.ExecuteAsync();

                foreach (var state in statuses.Get)
                {
                    ProgressSubject.OnNext(state);
                    ViewModel.IsRunningSync.Should().BeFalse();
                    ViewModel.IsSynced.Should().BeFalse();
                }
            }
        }

        public sealed class TheLogoutCommand : SettingsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task SetsTheIsLoggingOutFlagToTrue()
            {
                doNotShowConfirmationDialog();
                await ViewModel.LogoutCommand.ExecuteAsync();

                ViewModel.IsLoggingOut.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public async Task CallsLogoutOnTheDataSource()
            {
                doNotShowConfirmationDialog();
                await ViewModel.LogoutCommand.ExecuteAsync();

                await DataSource.Received().Logout();
            }

            [Fact, LogIfTooSlow]
            public async Task ResetsUserPreferences()
            {
                doNotShowConfirmationDialog();
                await ViewModel.LogoutCommand.ExecuteAsync();

                UserPreferences.Received().Reset();
            }

            [Fact, LogIfTooSlow]
            public async Task NavigatesToTheOnboardingScreen()
            {
                doNotShowConfirmationDialog();
                await ViewModel.LogoutCommand.ExecuteAsync();

                await NavigationService.Received().Navigate<OnboardingViewModel>();
            }

            [Fact, LogIfTooSlow]
            public void ChecksIfThereAreUnsyncedDataWhenTheSyncProcessFinishes()
            {
                ProgressSubject.OnNext(SyncProgress.Synced);

                DataSource.Received().HasUnsyncedData();
            }

            [Fact, LogIfTooSlow]
            public void SetsTheIsSyncedFlagAfterTheSyncProcessHasFinishedAndThereIsNoTimeEntryToPush()
            {
                DataSource.HasUnsyncedData().Returns(Observable.Return(false));
                ProgressSubject.OnNext(SyncProgress.Synced);

                ViewModel.IsSynced.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public void UnsetsTheIsSyncedFlagWhenTheSyncProcessIsNotRunningButThrereIsSomeTimeEntryToPush()
            {
                DataSource.HasUnsyncedData().Returns(Observable.Return(true));
                ProgressSubject.OnNext(SyncProgress.Synced);

                ViewModel.IsSynced.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public void UnsetsTheIsSyncedFlagWhenThereIsNothingToPushButTheSyncProcessStartsAgain()
            {
                DataSource.HasUnsyncedData().Returns(Observable.Return(false));
                ProgressSubject.OnNext(SyncProgress.Synced);
                ProgressSubject.OnNext(SyncProgress.Syncing);

                ViewModel.IsSynced.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public async Task DoesNotShowConfirmationDialogWhenTheAppIsInSync()
            {
                doNotShowConfirmationDialog();

                await ViewModel.LogoutCommand.ExecuteAsync();

                await DialogService.DidNotReceiveWithAnyArgs().Confirm("", "", "", "");
            }

            [Fact, LogIfTooSlow]
            public async Task ShowsConfirmationDialogWhenThereIsNothingToPushButSyncIsRunning()
            {
                DataSource.HasUnsyncedData().Returns(Observable.Return(false));
                ProgressSubject.OnNext(SyncProgress.Syncing);

                await ViewModel.LogoutCommand.ExecuteAsync();

                await DialogService.ReceivedWithAnyArgs().Confirm("", "", "", "");
            }

            [Fact, LogIfTooSlow]
            public async Task ShowsConfirmationDialogWhenThereIsSomethingToPushButSyncIsNotRunning()
            {
                DataSource.HasUnsyncedData().Returns(Observable.Return(true));
                ProgressSubject.OnNext(SyncProgress.Syncing);

                await ViewModel.LogoutCommand.ExecuteAsync();

                await DialogService.ReceivedWithAnyArgs().Confirm("", "", "", "");
            }

            [Fact, LogIfTooSlow]
            public async Task DoesNotProceedWithLogoutWhenUserClicksCancelButtonInTheDialog()
            {
                ProgressSubject.OnNext(SyncProgress.Syncing);
                DialogService.Confirm(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>()).Returns(false);

                await ViewModel.LogoutCommand.ExecuteAsync();

                ViewModel.IsLoggingOut.Should().BeFalse();
                await DataSource.DidNotReceive().Logout();
                await NavigationService.DidNotReceive().Navigate<OnboardingViewModel>();
            }

            [Fact, LogIfTooSlow]
            public async Task ProceedsWithLogoutWhenUserClicksSignOutButtonInTheDialog()
            {
                ProgressSubject.OnNext(SyncProgress.Syncing);
                DialogService.Confirm(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>()).Returns(true);

                await ViewModel.LogoutCommand.ExecuteAsync();

                ViewModel.IsLoggingOut.Should().BeTrue();
                await DataSource.Received().Logout();
                await NavigationService.Received().Navigate<OnboardingViewModel>();
            }

            [Fact, LogIfTooSlow]
            public async Task TracksLogoutEvent()
            {
                doNotShowConfirmationDialog();
                await ViewModel.LogoutCommand.ExecuteAsync();

                AnalyticsService.Received().TrackLogoutEvent(Analytics.LogoutSource.Settings);
            }

            private void doNotShowConfirmationDialog()
            {
                DataSource.HasUnsyncedData().Returns(Observable.Return(false));
                ProgressSubject.OnNext(SyncProgress.Synced);
            }
        }

        public sealed class ThePickWorkspaceCommand : SettingsViewModelTest
        {
            private const long workspaceId = 10;
            private const long defaultWorkspaceId = 11;
            private const string workspaceName = "My custom workspace";
            private readonly IDatabaseWorkspace workspace = Substitute.For<IDatabaseWorkspace>();
            private readonly IDatabaseWorkspace defaultWorkspace = Substitute.For<IDatabaseWorkspace>();

            public ThePickWorkspaceCommand()
            {
                workspace.Id.Returns(workspaceId);
                workspace.Name.Returns(workspaceName);
                defaultWorkspace.Id.Returns(defaultWorkspaceId);

                InteractorFactory.GetDefaultWorkspace().Execute()
                    .Returns(Observable.Return(defaultWorkspace));

                InteractorFactory.GetWorkspaceById(workspaceId).Execute()
                    .Returns(Observable.Return(workspace));

                ViewModel.Prepare();
            }

            [Fact, LogIfTooSlow]
            public async Task CallsTheSelectWorkspaceViewModel()
            {
                await ViewModel.PickWorkspaceCommand.ExecuteAsync();

                await NavigationService.Received()
                    .Navigate<SelectWorkspaceViewModel, WorkspaceParameters, long>(Arg.Any<WorkspaceParameters>());
            }

            [Fact, LogIfTooSlow]
            public async Task SetsTheReturnedWorkspaceNameAsTheWorkspaceNameProperty()
            {
                NavigationService
                    .Navigate<SelectWorkspaceViewModel, WorkspaceParameters, long>(Arg.Any<WorkspaceParameters>())
                    .Returns(Task.FromResult(workspaceId));

                await ViewModel.PickWorkspaceCommand.ExecuteAsync();

                ViewModel.WorkspaceName.Should().Be(workspaceName);
            }

            [Fact, LogIfTooSlow]
            public async Task UpdatesTheUserWithTheReceivedWorspace()
            {
                NavigationService
                    .Navigate<SelectWorkspaceViewModel, WorkspaceParameters, long>(Arg.Any<WorkspaceParameters>())
                    .Returns(Task.FromResult(workspaceId));

                await ViewModel.PickWorkspaceCommand.ExecuteAsync();

                await DataSource.User.Received().UpdateWorkspace(Arg.Is(workspaceId));
            }

            [Fact, LogIfTooSlow]
            public async Task StartsTheSyncAlgorithm()
            {
                NavigationService
                    .Navigate<SelectWorkspaceViewModel, WorkspaceParameters, long>(Arg.Any<WorkspaceParameters>())
                    .Returns(Task.FromResult(workspaceId));

                await ViewModel.PickWorkspaceCommand.ExecuteAsync();

                await DataSource.SyncManager.Received().PushSync();
            }
        }

        public sealed class TheBackCommand : SettingsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task ClosesTheViewModel()
            {
                await ViewModel.BackCommand.ExecuteAsync();

                await NavigationService.Received().Close(ViewModel);
            }
        }

        public sealed class TheToggleAddMobileTagCommand : SettingsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void TogglesTheCurrentValueOfTheToggleAddMobileTagProperty()
            {
                var expected = !ViewModel.AddMobileTag;

                ViewModel.ToggleAddMobileTagCommand.Execute();

                ViewModel.AddMobileTag.Should().Be(expected);
            }
        }

        public sealed class TheToggleManualModeCommand : SettingsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task ChangesManualModeToTimerMode()
            {
                UserPreferences.IsManualModeEnabled().Returns(true);

                await ViewModel.Initialize();
                ViewModel.ToggleManualModeCommand.Execute();

                ViewModel.IsManualModeEnabled.Should().BeFalse();
                UserPreferences.Received().EnableTimerMode();
            }

            [Fact, LogIfTooSlow]
            public async Task ChangesTimerModeToManualMode()
            {
                UserPreferences.IsManualModeEnabled().Returns(true);

                await ViewModel.Initialize();
                ViewModel.ToggleManualModeCommand.Execute();

                ViewModel.IsManualModeEnabled.Should().BeFalse();
                UserPreferences.Received().EnableTimerMode();
            }
        }


        public sealed class TheToggleUseTwentyFourHourClockCommand : SettingsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void TogglesTheCurrentValueOfTheToggleUseTwentyFourHourClockProperty()
            {
                var expected = !ViewModel.UseTwentyFourHourClock;

                ViewModel.ToggleUseTwentyFourHourClockCommand.Execute();

                ViewModel.UseTwentyFourHourClock.Should().Be(expected);
            }
        }

        public sealed class TheHelpCommand : SettingsViewModelTest
        {
            [Property]
            public void NavigatesToBrowserViewModelWithUrlFromPlatformConstants(
                NonEmptyString nonEmptyString)
            {
                var helpUrl = nonEmptyString.Get;
                PlatformConstants.HelpUrl.Returns(helpUrl);

                ViewModel.HelpCommand.Execute();

                NavigationService.Received().Navigate<BrowserViewModel, BrowserParameters>(
                    Arg.Is<BrowserParameters>(parameter => parameter.Url == helpUrl));
            }

            [Fact, LogIfTooSlow]
            public void NavigatesToBrowserViewModelWithHelpTitle()
            {
                ViewModel.HelpCommand.Execute();

                NavigationService.Received().Navigate<BrowserViewModel, BrowserParameters>(
                    Arg.Is<BrowserParameters>(parameter => parameter.Title == Resources.Help));
            }
        }

        public sealed class TheSubmitFeedbackCommand : SettingsViewModelTest
        {
            [Property]
            public void SendsAnEmailToTogglSupport(
                NonEmptyString nonEmptyString0, NonEmptyString nonEmptyString1)
            {
                var phoneModel = nonEmptyString0.Get;
                var os = nonEmptyString1.Get;
                PlatformConstants.PhoneModel.Returns(phoneModel);
                PlatformConstants.OperatingSystem.Returns(os);

                ViewModel.SubmitFeedbackCommand.Execute();

                MailService
                    .Received()
                    .Send(
                        "support@toggl.com",
                        Arg.Any<string>(),
                        Arg.Any<string>())
                    .Wait();
            }

            [Property]
            public void SendsAnEmailWithTheProperSubject(
                NonEmptyString nonEmptyString)
            {
                var subject = nonEmptyString.Get;
                PlatformConstants.FeedbackEmailSubject.Returns(subject);

                ViewModel.SubmitFeedbackCommand.ExecuteAsync().Wait();

                MailService.Received()
                    .Send(
                        Arg.Any<string>(),
                        subject,
                        Arg.Any<string>())
                   .Wait();
            }

            [Fact, LogIfTooSlow]
            public async Task SendsAnEmailWithAppVersionPhoneModelAndOsVersion()
            {
                PlatformConstants.PhoneModel.Returns("iPhone Y");
                PlatformConstants.OperatingSystem.Returns("iOS 4.2.0");
                var expectedMessage = $"\n\nVersion: {UserAgent.ToString()}\nPhone: {PlatformConstants.PhoneModel}\nOS: {PlatformConstants.OperatingSystem}";

                await ViewModel.SubmitFeedbackCommand.ExecuteAsync();

                await MailService.Received().Send(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    expectedMessage);
            }

            [Property]
            public void AlertsUserWhenMailServiceReturnsAnError(
                NonEmptyString nonEmptyString0, NonEmptyString nonEmptyString1)
            {
                var errorTitle = nonEmptyString0.Get;
                var errorMessage = nonEmptyString1.Get;
                var result = new MailResult(false, errorTitle, errorMessage);
                MailService
                    .Send(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                    .Returns(Task.FromResult(result));

                ViewModel.SubmitFeedbackCommand.Execute();

                DialogService
                    .Received()
                    .Alert(errorTitle, errorMessage, Resources.Ok)
                    .Wait();
            }

            [Theory, LogIfTooSlow]
            [InlineData(true, "")]
            [InlineData(true, "Error")]
            [InlineData(true, null)]
            [InlineData(false, "")]
            [InlineData(false, null)]
            public async Task DoesNotAlertUserWhenMailServiceReturnsSuccessOrDoesNotHaveErrorTitle(
                bool success, string errorTitle)
            {
                var result = new MailResult(success, errorTitle, "");
                MailService
                    .Send(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                    .Returns(Task.FromResult(result));

                await ViewModel.SubmitFeedbackCommand.ExecuteAsync();

                await DialogService
                    .DidNotReceive()
                    .Alert(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            }
        }

        public sealed class TheInitializeMethod : SettingsViewModelTest
        {
            [Fact]
            public async Task InitializesFormatsFromPreferencesDataSource()
            {
                var preferences = createPreferences();
                DataSource.Preferences.Current.Returns(Observable.Return(preferences));

                await ViewModel.Initialize();

                ViewModel.DateFormat.Should().Be(preferences.DateFormat);
                ViewModel.UseTwentyFourHourClock.Should().Be(preferences.TimeOfDayFormat.IsTwentyFourHoursFormat);
                ViewModel.DurationFormat.Should().Be(preferences.DurationFormat);
            }

            [Fact, LogIfTooSlow]
            public async Task InitializesVersion()
            {
                await ViewModel.Initialize();

                ViewModel.Version.Should().Be(UserAgent.Version);
            }

            private IDatabasePreferences createPreferences()
            {
                var preferences = Substitute.For<IDatabasePreferences>();
                preferences.DateFormat.Returns(DateFormat.FromLocalizedDateFormat("MM.DD.YYYY"));
                preferences.DurationFormat.Returns(DurationFormat.Classic);
                preferences.TimeOfDayFormat.Returns(TimeFormat.TwelveHoursFormat);
                return preferences;
            }
        }

        public sealed class TheSelectDateFormatCommand : SettingsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task NavigatesToSelectDateFormatViewModelPassingCurrentDateFormat()
            {
                var dateFormat = DateFormat.FromLocalizedDateFormat("MM-DD-YYYY");
                var preferences = Substitute.For<IDatabasePreferences>();
                preferences.DateFormat.Returns(dateFormat);
                DataSource.Preferences.Current.Returns(Observable.Return(preferences));
                await ViewModel.Initialize();

                await ViewModel.SelectDateFormatCommand.ExecuteAsync();

                await NavigationService
                    .Received()
                    .Navigate<SelectDateFormatViewModel, DateFormat, DateFormat>(dateFormat);
            }

            [Fact, LogIfTooSlow]
            public async Task UpdatesTheStoredPreferences()
            {
                var oldDateFormat = DateFormat.FromLocalizedDateFormat("MM-DD-YYYY");
                var newDateFormat = DateFormat.FromLocalizedDateFormat("DD.MM.YYYY");
                var preferences = Substitute.For<IDatabasePreferences>();
                preferences.DateFormat.Returns(oldDateFormat);
                DataSource.Preferences.Current.Returns(Observable.Return(preferences));
                NavigationService
                    .Navigate<SelectDateFormatViewModel, DateFormat, DateFormat>(Arg.Any<DateFormat>())
                    .Returns(Task.FromResult(newDateFormat));
                await ViewModel.Initialize();

                await ViewModel.SelectDateFormatCommand.ExecuteAsync();

                await DataSource
                    .Preferences
                    .Received()
                    .Update(Arg.Is<EditPreferencesDTO>(dto => dto.DateFormat == newDateFormat));
            }

            [Fact, LogIfTooSlow]
            public async Task UpdatesTheDateFormatProperty()
            {
                var oldDateFormat = DateFormat.FromLocalizedDateFormat("MM-DD-YYYY");
                var newDateFormat = DateFormat.FromLocalizedDateFormat("DD.MM.YYYY");
                var oldPreferences = Substitute.For<IDatabasePreferences>();
                oldPreferences.DateFormat.Returns(oldDateFormat);
                var newPreferences = Substitute.For<IDatabasePreferences>();
                newPreferences.DateFormat.Returns(newDateFormat);
                DataSource.Preferences.Current.Returns(Observable.Return(oldPreferences));
                NavigationService
                    .Navigate<SelectDateFormatViewModel, DateFormat, DateFormat>(Arg.Any<DateFormat>())
                    .Returns(Task.FromResult(newDateFormat));
                DataSource
                    .Preferences
                    .Update(Arg.Any<EditPreferencesDTO>())
                    .Returns(Observable.Return(newPreferences));
                await ViewModel.Initialize();

                await ViewModel.SelectDateFormatCommand.ExecuteAsync();

                ViewModel.DateFormat.Should().Be(newDateFormat);
            }

            [Fact, LogIfTooSlow]
            public async Task InitiatesPushSync()
            {
                var oldDateFormat = DateFormat.FromLocalizedDateFormat("MM-DD-YYYY");
                var newDateFormat = DateFormat.FromLocalizedDateFormat("DD.MM.YYYY");
                var preferences = Substitute.For<IDatabasePreferences>();
                preferences.DateFormat.Returns(oldDateFormat);
                DataSource.Preferences.Get().Returns(Observable.Return(preferences));
                NavigationService
                    .Navigate<SelectDateFormatViewModel, DateFormat, DateFormat>(Arg.Any<DateFormat>())
                    .Returns(Task.FromResult(newDateFormat));
                await ViewModel.Initialize();

                await ViewModel.SelectDateFormatCommand.ExecuteAsync();

                await DataSource.SyncManager.Received().PushSync();
            }
        }

        public sealed class TheToggleUseTwentyFourHourClock : SettingsViewModelTest
        {
            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public async Task ChangesTheValueOfTheUseTwentyFourHourHourClock(bool originalValue)
            {
                await ViewModel.Initialize();
                ViewModel.UseTwentyFourHourClock = originalValue;

                await ViewModel.ToggleUseTwentyFourHourClockCommand.ExecuteAsync();

                ViewModel.UseTwentyFourHourClock.Should().Be(!originalValue);
            }

            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public async Task UpdatesTheValueInTheDataSource(bool originalValue)
            {
                await ViewModel.Initialize();
                ViewModel.UseTwentyFourHourClock = originalValue;

                await ViewModel.ToggleUseTwentyFourHourClockCommand.ExecuteAsync();

                await DataSource.Preferences.Received().Update(Arg.Is<EditPreferencesDTO>(
                    dto => dto.TimeOfDayFormat.HasValue
                        && dto.TimeOfDayFormat.Value.IsTwentyFourHoursFormat != originalValue));
            }

            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public async Task InitiatesPushSync(bool originalValue)
            {
                var preferences = Substitute.For<IDatabasePreferences>();
                var observable = Observable.Return(preferences);
                DataSource.Preferences.Update(Arg.Any<EditPreferencesDTO>()).Returns(observable);
                await ViewModel.Initialize();
                ViewModel.UseTwentyFourHourClock = originalValue;

                await ViewModel.ToggleUseTwentyFourHourClockCommand.ExecuteAsync();

                await DataSource.SyncManager.Received().PushSync();
            }
        }

        public sealed class TheSelectDurationFormatCommand : SettingsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task NavigatesToSelectDurationFormatViewModelPassingCurrentDurationFormat()
            {
                var durationFormat = DurationFormat.Improved;
                var preferences = Substitute.For<IDatabasePreferences>();
                preferences.DurationFormat.Returns(durationFormat);
                DataSource.Preferences.Current.Returns(Observable.Return(preferences));
                await ViewModel.Initialize();

                await ViewModel.SelectDurationFormatCommand.ExecuteAsync();

                await NavigationService
                    .Received()
                    .Navigate<SelectDurationFormatViewModel, DurationFormat, DurationFormat>(durationFormat);
            }

            [Fact, LogIfTooSlow]
            public async Task UpdatesTheStoredPreferences()
            {
                var oldDurationFormat = DurationFormat.Decimal;
                var newDurationFormat = DurationFormat.Improved;
                var preferences = Substitute.For<IDatabasePreferences>();
                preferences.DurationFormat.Returns(oldDurationFormat);
                DataSource.Preferences.Current.Returns(Observable.Return(preferences));
                NavigationService
                    .Navigate<SelectDurationFormatViewModel, DurationFormat, DurationFormat>(Arg.Any<DurationFormat>())
                    .Returns(Task.FromResult(newDurationFormat));
                await ViewModel.Initialize();

                await ViewModel.SelectDurationFormatCommand.ExecuteAsync();

                await DataSource
                    .Preferences
                    .Received()
                    .Update(Arg.Is<EditPreferencesDTO>(dto => dto.DurationFormat == newDurationFormat));
            }

            [Fact, LogIfTooSlow]
            public async Task SelectDurationFormatCommandCallsPushSync()
            {
                var oldDurationFormat = DurationFormat.Decimal;
                var newDurationFormat = DurationFormat.Improved;
                var preferences = Substitute.For<IDatabasePreferences>();
                preferences.DurationFormat.Returns(oldDurationFormat);
                DataSource.Preferences.Current.Returns(Observable.Return(preferences));
                NavigationService
                    .Navigate<SelectDurationFormatViewModel, DurationFormat, DurationFormat>(Arg.Any<DurationFormat>())
                    .Returns(Task.FromResult(newDurationFormat));
                var syncManager = Substitute.For<ISyncManager>();
                DataSource.SyncManager.Returns(syncManager);
                await ViewModel.Initialize();

                await ViewModel.SelectDurationFormatCommand.ExecuteAsync();

                await syncManager.Received().PushSync();
            }

            [Fact, LogIfTooSlow]
            public async Task UpdatesTheDurationFormatProperty()
            {
                var oldDurationFormat = DurationFormat.Decimal;
                var newDurationFormat = DurationFormat.Improved;
                var oldPreferences = Substitute.For<IDatabasePreferences>();
                oldPreferences.DurationFormat.Returns(oldDurationFormat);
                var newPreferences = Substitute.For<IDatabasePreferences>();
                newPreferences.DurationFormat.Returns(newDurationFormat);
                DataSource.Preferences.Current.Returns(Observable.Return(oldPreferences));
                NavigationService
                    .Navigate<SelectDurationFormatViewModel, DurationFormat, DurationFormat>(Arg.Any<DurationFormat>())
                    .Returns(Task.FromResult(newDurationFormat));
                DataSource
                    .Preferences
                    .Update(Arg.Any<EditPreferencesDTO>())
                    .Returns(Observable.Return(newPreferences));
                await ViewModel.Initialize();

                await ViewModel.SelectDurationFormatCommand.ExecuteAsync();

                ViewModel.DurationFormat.Should().Be(newDurationFormat);
            }
        }

        public sealed class TheSelectBeginningOfWeekCommand : SettingsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task NavigatesToSelectBeginningOfWeekViewModelPassingCurrentBeginningOfWeek()
            {
                var beginningOfWeek = BeginningOfWeek.Friday;
                var user = Substitute.For<IDatabaseUser>();
                user.BeginningOfWeek.Returns(beginningOfWeek);
                DataSource.User.Current.Returns(Observable.Return(user));
                await ViewModel.Initialize();

                await ViewModel.SelectBeginningOfWeekCommand.ExecuteAsync();

                await NavigationService
                    .Received()
                    .Navigate<SelectBeginningOfWeekViewModel, BeginningOfWeek, BeginningOfWeek>(beginningOfWeek);
            }

            [Fact, LogIfTooSlow]
            public async Task UpdatesTheStoredPreferences()
            {
                var oldBeginningOfWeek = BeginningOfWeek.Tuesday;
                var newBeginningOfWeek = BeginningOfWeek.Sunday;

                var user = Substitute.For<IDatabaseUser>();
                user.BeginningOfWeek.Returns(oldBeginningOfWeek);
                DataSource.User.Current.Returns(Observable.Return(user));
                NavigationService
                    .Navigate<SelectBeginningOfWeekViewModel, BeginningOfWeek, BeginningOfWeek>(Arg.Any<BeginningOfWeek>())
                    .Returns(Task.FromResult(newBeginningOfWeek));
                await ViewModel.Initialize();

                await ViewModel.SelectBeginningOfWeekCommand.ExecuteAsync();

                await DataSource
                    .User
                    .Received()
                    .Update(Arg.Is<EditUserDTO>(dto => dto.BeginningOfWeek == newBeginningOfWeek));
            }

            [Fact, LogIfTooSlow]
            public async Task SelectBeginningOfWeekCommandCallsPushSync()
            {
                var oldBeginningOfWeek = BeginningOfWeek.Tuesday;
                var newBeginningOfWeek = BeginningOfWeek.Sunday;
                var user = Substitute.For<IDatabaseUser>();
                user.BeginningOfWeek.Returns(oldBeginningOfWeek);
                DataSource.User.Current.Returns(Observable.Return(user));
                NavigationService
                    .Navigate<SelectBeginningOfWeekViewModel, BeginningOfWeek, BeginningOfWeek>(Arg.Any<BeginningOfWeek>())
                    .Returns(Task.FromResult(newBeginningOfWeek));
                var syncManager = Substitute.For<ISyncManager>();
                DataSource.SyncManager.Returns(syncManager);
                await ViewModel.Initialize();

                await ViewModel.SelectBeginningOfWeekCommand.ExecuteAsync();

                await syncManager.Received().PushSync();
            }

            [Fact, LogIfTooSlow]
            public async Task UpdatesTheBeginningOfWeekProperty()
            {
                var oldBeginningOfWeek = BeginningOfWeek.Tuesday;
                var newBeginningOfWeek = BeginningOfWeek.Sunday;

                var oldUser = Substitute.For<IDatabaseUser>();
                oldUser.BeginningOfWeek.Returns(oldBeginningOfWeek);
                var newUser = Substitute.For<IDatabaseUser>();
                newUser.BeginningOfWeek.Returns(newBeginningOfWeek);
                DataSource.User.Current.Returns(Observable.Return(oldUser));
                NavigationService
                    .Navigate<SelectBeginningOfWeekViewModel, BeginningOfWeek, BeginningOfWeek>(Arg.Any<BeginningOfWeek>())
                    .Returns(Task.FromResult(newBeginningOfWeek));
                DataSource
                    .User
                    .Update(Arg.Any<EditUserDTO>())
                    .Returns(Observable.Return(newUser));
                await ViewModel.Initialize();

                await ViewModel.SelectBeginningOfWeekCommand.ExecuteAsync();

                ViewModel.BeginningOfWeek.Should().Be(newBeginningOfWeek);
            }
        }

        public sealed class TheAboutCommand : SettingsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task NavigatesToTheAboutPage()
            {
                await ViewModel.AboutCommand.ExecuteAsync();

                await NavigationService.Received().Navigate<AboutViewModel>();
            }
        }
    }
}
