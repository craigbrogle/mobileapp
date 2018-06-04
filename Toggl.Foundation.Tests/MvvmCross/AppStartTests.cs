using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Login;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.MvvmCross;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant.Models;
using Toggl.PrimeRadiant.Settings;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross
{
    public sealed class AppStartTests
    {
        public abstract class AppStartTest : BaseMvvmCrossTests
        {
            protected AppStart<OnboardingViewModel> AppStart { get; }
            protected ISyncManager SyncManager { get; } = Substitute.For<ISyncManager>();
            protected ILoginManager LoginManager { get; } = Substitute.For<ILoginManager>();
            protected IAccessRestrictionStorage AccessRestrictionStorage { get; } =
                Substitute.For<IAccessRestrictionStorage>();

            protected AppStartTest()
            {
                AppStart = new AppStart<OnboardingViewModel>(LoginManager, NavigationService, AccessRestrictionStorage);
                DataSource.SyncManager.Returns(SyncManager);
                LoginManager.GetDataSourceIfLoggedIn().Returns(DataSource);
            }
        }

        public sealed class TheConstructor : AppStartTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(ThreeParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool userLoginManager, bool userNavigationService, bool useAccessRestrictionStorage)
            {
                var loginManager = userLoginManager ? LoginManager : null;
                var navigationService = userNavigationService ? NavigationService : null;
                var accessRestrictionStorage = useAccessRestrictionStorage ? AccessRestrictionStorage : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new AppStart<OnboardingViewModel>(loginManager, navigationService, accessRestrictionStorage);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheStartMethod : AppStartTest
        {
            [Fact, LogIfTooSlow]
            public async Task ShowsTheOutdatedViewIfTheCurrentVersionOfTheAppIsOutdated()
            {
                AccessRestrictionStorage.IsClientOutdated().Returns(true);

                await Task.Run(() => AppStart.Start());

                await NavigationService.Received().Navigate<OnboardingViewModel>();
                await NavigationService.Received().Navigate<OutdatedAppViewModel>();
                LoginManager.DidNotReceive().GetDataSourceIfLoggedIn();
            }

            [Fact, LogIfTooSlow]
            public async Task ShowsTheOutdatedViewIfTheVersionOfTheCurrentlyUsedApiIsOutdated()
            {
                AccessRestrictionStorage.IsApiOutdated().Returns(true);

                await Task.Run(() => AppStart.Start());

                await NavigationService.Received().Navigate<OnboardingViewModel>();
                await NavigationService.Received().Navigate<OutdatedAppViewModel>();
                LoginManager.DidNotReceive().GetDataSourceIfLoggedIn();
            }

            [Fact, LogIfTooSlow]
            public async Task ShowsTheReLoginViewIfTheUserRevokedTheApiToken()
            {
                AccessRestrictionStorage.IsUnauthorized(Arg.Any<string>()).Returns(true);

                await Task.Run(() => AppStart.Start());

                await SyncManager.DidNotReceive().ForceFullSync();
                await NavigationService.Received().Navigate<TokenResetViewModel>();
            }

            [Fact, LogIfTooSlow]
            public async Task ShowsTheOutdatedViewIfTheTokenWasRevokedAndTheAppIsOutdated()
            {
                AccessRestrictionStorage.IsUnauthorized(Arg.Any<string>()).Returns(true);
                AccessRestrictionStorage.IsClientOutdated().Returns(true);

                await Task.Run(() => AppStart.Start());

                await NavigationService.Received().Navigate<OnboardingViewModel>();
                await NavigationService.Received().Navigate<OutdatedAppViewModel>();
                LoginManager.DidNotReceive().GetDataSourceIfLoggedIn();
            }

            [Fact, LogIfTooSlow]
            public async Task ShowsTheOutdatedViewIfTheTokenWasRevokedAndTheApiIsOutdated()
            {
                AccessRestrictionStorage.IsUnauthorized(Arg.Any<string>()).Returns(true);
                AccessRestrictionStorage.IsApiOutdated().Returns(true);

                await Task.Run(() => AppStart.Start());

                await NavigationService.Received().Navigate<OnboardingViewModel>();
                await NavigationService.Received().Navigate<OutdatedAppViewModel>();
                await NavigationService.DidNotReceive().Navigate<TokenResetViewModel>();
                LoginManager.DidNotReceive().GetDataSourceIfLoggedIn();
            }

            [Fact, LogIfTooSlow]
            public async Task DoesNotShowTheUnauthorizedAccessViewIfUsersApiTokenChanged()
            {
                var oldApiToken = Guid.NewGuid().ToString();
                var newApiToken = Guid.NewGuid().ToString();
                var user = Substitute.For<IThreadSafeUser>();
                var dataSource = Substitute.For<ITogglDataSource>();
                var userSource = Substitute.For<IUserSource>();
                user.ApiToken.Returns(newApiToken);
                userSource.Current.Returns(Observable.Return(user));
                dataSource.User.Returns(userSource);
                LoginManager.GetDataSourceIfLoggedIn().Returns(dataSource);
                AccessRestrictionStorage.IsUnauthorized(Arg.Is(oldApiToken)).Returns(true);
                AccessRestrictionStorage.IsApiOutdated().Returns(false);
                AccessRestrictionStorage.IsClientOutdated().Returns(false);

                await Task.Run(() => AppStart.Start());

                await NavigationService.Received().Navigate<MainViewModel>();
            }

            [Fact, LogIfTooSlow]
            public async Task ShowsTheOnboardingViewModelIfTheUserHasNotLoggedInPreviously()
            {
                ITogglDataSource dataSource = null;
                LoginManager.GetDataSourceIfLoggedIn().Returns(dataSource);

                await Task.Run(() => AppStart.Start());

                await NavigationService.Received().Navigate<OnboardingViewModel>();
            }

            [Fact, LogIfTooSlow]
            public async Task ShowsTheTimeEntriesViewModelIfTheUserHasLoggedInPreviously()
            {
                var dataSource = Substitute.For<ITogglDataSource>();
                LoginManager.GetDataSourceIfLoggedIn().Returns(dataSource);

                await Task.Run(() => AppStart.Start());

                await NavigationService.Received().Navigate<MainViewModel>();
            }
        }
    }
}
