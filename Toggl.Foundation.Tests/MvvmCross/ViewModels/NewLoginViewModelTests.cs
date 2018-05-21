using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck.Xunit;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.Login;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Services;
using Toggl.Foundation.Tests.Generators;
using Toggl.Multivac;
using Toggl.Ultrawave.Exceptions;
using Toggl.Ultrawave.Network;
using Xunit;
using Toggl.Foundation.Tests.TestExtensions;
using FsCheck;
using Toggl.Foundation.MvvmCross.Parameters;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class NewLoginViewModelTests
    {
        public abstract class NewLoginViewModelTest : BaseViewModelTests<NewLoginViewModel>
        {
            protected Email ValidEmail { get; } = Email.From("person@company.com");
            protected Email InvalidEmail { get; } = Email.From("this is not an email");

            protected Password ValidPassword { get; } = Password.From("T0t4lly s4afe p4$$");
            protected Password InvalidPassword { get; } = Password.From("123");

            protected override NewLoginViewModel CreateViewModel()
                => new NewLoginViewModel(
                    LoginManager,
                    AnalyticsService,
                    OnboardingStorage,
                    NavigationService,
                    PasswordManagerService,
                    ApiErrorHandlingService);
        }

        public sealed class TheConstructor : NewLoginViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(SixParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool userLoginManager,
                bool useAnalyticsService,
                bool useOnboardingStorage,
                bool userNavigationService,
                bool usePasswordManagerService,
                bool useApiErrorHandlingService)
            {
                var loginManager = userLoginManager ? LoginManager : null;
                var analyticsSerivce = useAnalyticsService ? AnalyticsService : null;
                var onboardingStorage = useOnboardingStorage ? OnboardingStorage : null;
                var navigationService = userNavigationService ? NavigationService : null;
                var passwordManagerService = usePasswordManagerService ? PasswordManagerService : null;
                var apiErrorHandlingService = useApiErrorHandlingService ? ApiErrorHandlingService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new LoginViewModel(loginManager,
                                             onboardingStorage,
                                             navigationService,
                                             passwordManagerService,
                                             apiErrorHandlingService,
                                             analyticsSerivce);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheLoginEnabledProperty : NewLoginViewModelTest
        {
            [Theory]
            [InlineData("invalid email address", "123")]
            [InlineData("invalid email address", "T0tally s4afe p4a$$")]
            [InlineData("person@company.com", "123")]
            public void ReturnsFalseWhenEmailOrPasswordIsInvalid(string email, string password)
            {
                ViewModel.Email = Email.From(email);
                ViewModel.Password = Password.From(password);

                ViewModel.LoginEnabled.Should().BeFalse();
            }

            [Theory]
            [InlineData("invalid email address", "123")]
            [InlineData("invalid email address", "T0tally s4afe p4a$$")]
            [InlineData("person@company.com", "123")]
            [InlineData("person@company.com", "T0tally s4afe p4a$$")]
            public void ReturnsFalseWhenIsLoading(string email, string password)
            { 
                ViewModel.Email = Email.From(email);
                ViewModel.Password = Password.From(password);
                //Make sure isloading is true
                LoginManager
                    .Login(Arg.Any<Email>(), Arg.Any<Password>())
                    .Returns(Observable.Never<ITogglDataSource>());
                ViewModel.LoginCommand.Execute();

                ViewModel.LoginEnabled.Should().BeFalse();
            }
        }

        public sealed class TheIsPasswordManagerAvailableProperty : NewLoginViewModelTest
        {
            [Property]
            public void ReturnsWhetherThePasswordManagerIsAvailable(bool isAvailable)
            {
                PasswordManagerService.IsAvailable.Returns(isAvailable);

                ViewModel.IsPasswordManagerAvailable.Should().Be(isAvailable);
            }
        }

        public sealed class LoginCommand : NewLoginViewModelTest
        {
            [Theory]
            [InlineData("invalid email address", "123")]
            [InlineData("invalid email address", "T0tally s4afe p4a$$")]
            [InlineData("person@company.com", "123")]
            public void CannotBeExecutedWhenTheEmailOrPasswordIsInvalid(string email, string password)
            {
                ViewModel.Email = Email.From(email);
                ViewModel.Password = Password.From(password);

                ViewModel.LoginCommand.CanExecute().Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public void CallsTheLoginManagerWhenTheEmailAndPasswordAreValid()
            {
                ViewModel.Email = ValidEmail;
                ViewModel.Password = ValidPassword;

                ViewModel.LoginCommand.Execute();

                LoginManager.Received().Login(Arg.Is(ValidEmail), Arg.Is(ValidPassword));
            }

            [Fact, LogIfTooSlow]
            public void DoesNothingWhenThePageIsCurrentlyLoading()
            {
                var never = Observable.Never<ITogglDataSource>();
                LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>()).Returns(never);
                ViewModel.Email = ValidEmail;
                ViewModel.Password = ValidPassword;
                ViewModel.LoginCommand.Execute();

                ViewModel.LoginCommand.Execute();

                LoginManager.Received(1).Login(Arg.Any<Email>(), Arg.Any<Password>());
            }

            public sealed class WhenLoginSucceeds : NewLoginViewModelTest
            {
                [Fact, LogIfTooSlow]
                public async Task StartsSyncing()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.Password = ValidPassword;
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Return(DataSource));

                    ViewModel.LoginCommand.Execute();

                    await DataSource.Received().StartSyncing();
                }

                [Fact, LogIfTooSlow]
                public void SetsIsNewUserToFalse()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.Password = ValidPassword;
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Return(Substitute.For<ITogglDataSource>()));

                    ViewModel.LoginCommand.Execute();

                    OnboardingStorage.Received().SetIsNewUser(false);
                }

                [Fact, LogIfTooSlow]
                public void SetsIsLoadingToFalse()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.Password = ValidPassword;
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Return(Substitute.For<ITogglDataSource>()));

                    ViewModel.LoginCommand.Execute();

                    ViewModel.IsLoading.Should().BeFalse();
                }

                [Fact, LogIfTooSlow]
                public void NavigatesToTheTimeEntriesViewModel()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.Password = ValidPassword;
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Return(Substitute.For<ITogglDataSource>()));

                    ViewModel.LoginCommand.Execute();

                    NavigationService.Received().Navigate<MainViewModel>();
                }

                [Fact, LogIfTooSlow]
                public void TracksTheLoginEvent()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.Password = ValidPassword;
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Return(Substitute.For<ITogglDataSource>()));

                    ViewModel.LoginCommand.Execute();

                    AnalyticsService.Received().TrackLoginEvent(AuthenticationMethod.EmailAndPassword);
                }
            }

            public sealed class WhenLoginFails : NewLoginViewModelTest
            {
                [Fact, LogIfTooSlow]
                public void SetsIsLoadingToFalse()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.Password = ValidPassword;
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Throw<ITogglDataSource>(new Exception()));

                    ViewModel.LoginCommand.Execute();

                    ViewModel.IsLoading.Should().BeFalse();
                }

                [Fact, LogIfTooSlow]
                public void DoesNotNavigate()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.Password = ValidPassword;
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Throw<ITogglDataSource>(new Exception()));

                    ViewModel.LoginCommand.Execute();

                    NavigationService.DidNotReceive().Navigate<MainViewModel>();
                }

                [Fact, LogIfTooSlow]
                public void SetsTheErrorMessageToIncorrectEmailOrPasswordWhenReceivedUnauthorizedException()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.Password = ValidPassword;
                    var exception = new UnauthorizedException(
                        Substitute.For<IRequest>(), Substitute.For<IResponse>());
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Throw<ITogglDataSource>(exception));

                    ViewModel.LoginCommand.Execute();

                    ViewModel.ErrorMessage.Should().Be(Resources.IncorrectEmailOrPassword);
                }

                [Fact, LogIfTooSlow]
                public void SetsTheErrorMessageToNothingWhenGoogleLoginWasCanceled()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.Password = ValidPassword;
                    var exception = new GoogleLoginException(true);
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Throw<ITogglDataSource>(exception));

                    ViewModel.LoginCommand.Execute();

                    ViewModel.ErrorMessage.Should().BeEmpty();
                }

                [Fact, LogIfTooSlow]
                public void SetsTheErrorMessageToGenericLoginErrorForAnyOtherException()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.Password = ValidPassword;
                    var exception = new Exception();
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Throw<ITogglDataSource>(exception));

                    ViewModel.LoginCommand.Execute();

                    ViewModel.ErrorMessage.Should().Be(Resources.GenericLoginError);
                }

                [Fact, LogIfTooSlow]
                public void DoesNothingWhenApiErrorHandlingServiceHandlesTheException()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.Password = ValidPassword;
                    var exception = new Exception();
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Throw<ITogglDataSource>(exception));
                    ApiErrorHandlingService.TryHandleDeprecationError(Arg.Any<Exception>())
                        .Returns(true);

                    ViewModel.LoginCommand.Execute();

                    ViewModel.ErrorMessage.Should().BeEmpty();
                }
            }
        }

        public sealed class TheGoogleLoginCommand : NewLoginViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void CallsTheLoginManager()
            {
                ViewModel.GoogleLoginCommand.Execute();

                LoginManager.Received().LoginWithGoogle();
            }

            [Fact, LogIfTooSlow]
            public void DoesNothingWhenThePageIsCurrentlyLoading()
            {
                var never = Observable.Never<ITogglDataSource>();
                LoginManager.LoginWithGoogle().Returns(never);
                ViewModel.GoogleLoginCommand.Execute();

                ViewModel.GoogleLoginCommand.Execute();

                LoginManager.Received(1).LoginWithGoogle();
            }

            [Fact, LogIfTooSlow]
            public void NavigatesToTheTimeEntriesViewModelWhenTheLoginSucceeds()
            {
                LoginManager.LoginWithGoogle()
                    .Returns(Observable.Return(Substitute.For<ITogglDataSource>()));

                ViewModel.GoogleLoginCommand.Execute();

                NavigationService.Received().Navigate<MainViewModel>();
            }

            [Fact, LogIfTooSlow]
            public void StopsTheViewModelLoadStateWhenItCompletes()
            {
                LoginManager.LoginWithGoogle()
                    .Returns(Observable.Return(Substitute.For<ITogglDataSource>()));

                ViewModel.GoogleLoginCommand.Execute();

                ViewModel.IsLoading.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public void TracksGoogleLoginEvent()
            {
                LoginManager.LoginWithGoogle()
                    .Returns(Observable.Return(Substitute.For<ITogglDataSource>()));

                ViewModel.GoogleLoginCommand.Execute();

                AnalyticsService.Received().TrackLoginEvent(AuthenticationMethod.Google);
            }

            [Fact, LogIfTooSlow]
            public void StopsTheViewModelLoadStateWhenItErrors()
            {
                LoginManager.LoginWithGoogle()
                    .Returns(Observable.Throw<ITogglDataSource>(new GoogleLoginException(false)));

                ViewModel.GoogleLoginCommand.Execute();

                ViewModel.IsLoading.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public void DoesNotNavigateWhenTheLoginFails()
            {
                LoginManager.LoginWithGoogle()
                    .Returns(Observable.Throw<ITogglDataSource>(new GoogleLoginException(false)));

                ViewModel.GoogleLoginCommand.Execute();

                NavigationService.DidNotReceive().Navigate<MainViewModel>();
            }

            [Fact, LogIfTooSlow]
            public void DoesNotDisplayAnErrormessageWhenTheUserCancelsTheRequestOnTheGoogleService()
            {
                LoginManager.LoginWithGoogle()
                    .Returns(Observable.Throw<ITogglDataSource>(new GoogleLoginException(true)));

                ViewModel.GoogleLoginCommand.Execute();

                ViewModel.ErrorMessage.Should().BeEmpty();
            }
        }

        public sealed class TheTogglePasswordVisibilityCommand : NewLoginViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void SetsTheIsPasswordMaskedToFalseWhenItIsTrue()
            {
                ViewModel.TogglePasswordVisibilityCommand.Execute();

                ViewModel.IsPasswordMasked.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public void SetsTheIsPasswordMaskedToTrueWhenItIsFalse()
            {
                ViewModel.TogglePasswordVisibilityCommand.Execute();

                ViewModel.TogglePasswordVisibilityCommand.Execute();

                ViewModel.IsPasswordMasked.Should().BeTrue();
            }
        }

        public sealed class TheStartPasswordManagerCommand : NewLoginViewModelTest
        {
            public TheStartPasswordManagerCommand()
            {
                PasswordManagerService.IsAvailable.Returns(true);
            }

            [Fact, LogIfTooSlow]
            public void CannotBeExecutedWhenThePasswordManagerIsNotAvailable()
            {
                PasswordManagerService.IsAvailable.Returns(false);

                ViewModel.StartPasswordManagerCommand.CanExecute().Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public void CallsThePasswordManagerServiceWhenTheServiceIsAvailable()
            {
                PasswordManagerService.GetLoginInformation().Returns(Observable.Never<PasswordManagerResult>());

                ViewModel.StartPasswordManagerCommand.ExecuteAsync();

                PasswordManagerService.Received().GetLoginInformation();
            }

            [Fact, LogIfTooSlow]
            public void DoesNothingWhenCalledASecondTimeBeforeTheObservableFromTheFirstCallReturns()
            {
                var scheduler = new TestScheduler();
                var never = Observable.Never<PasswordManagerResult>();
                PasswordManagerService.GetLoginInformation().Returns(never);

                scheduler.Schedule(TimeSpan.FromTicks(20), () => ViewModel.StartPasswordManagerCommand.ExecuteAsync());
                scheduler.Schedule(TimeSpan.FromTicks(40), () => ViewModel.StartPasswordManagerCommand.ExecuteAsync());

                scheduler.Start();

                PasswordManagerService.Received(1).GetLoginInformation();
            }

            [Fact, LogIfTooSlow]
            public void CallsTheLoginCommandWhenValidCredentialsAreProvided()
            {
                var scheduler = new TestScheduler();
                var observable = arrangeCallToPasswordManagerWithValidCredentials();

                scheduler.Schedule(TimeSpan.FromTicks(20), () => ViewModel.StartPasswordManagerCommand.ExecuteAsync());

                scheduler.Start(
                    () => observable,
                    created: 0,
                    subscribed: 10,
                    disposed: 100
                );

                LoginManager.Received().Login(Arg.Any<Email>(), Arg.Any<Password>());
            }

            [Fact, LogIfTooSlow]
            public void SetsTheEmailFieldWhenValidCredentialsAreProvided()
            {
                var scheduler = new TestScheduler();
                var observable = arrangeCallToPasswordManagerWithValidCredentials();

                scheduler.Schedule(TimeSpan.FromTicks(20), () => ViewModel.StartPasswordManagerCommand.ExecuteAsync());

                scheduler.Start(
                    () => observable,
                    created: 0,
                    subscribed: 10,
                    disposed: 100
                );

                ViewModel.Email.Should().Be(ValidEmail);
            }

            [Fact, LogIfTooSlow]
            public void SetsTheEmailFieldWhenInvalidCredentialsAreProvided()
            {
                var scheduler = new TestScheduler();
                var observable = arrangeCallToPasswordManagerWithInvalidCredentials();

                scheduler.Schedule(TimeSpan.FromTicks(20), () => ViewModel.StartPasswordManagerCommand.ExecuteAsync());

                scheduler.Start(
                    () => observable,
                    created: 0,
                    subscribed: 10,
                    disposed: 100
                );

                ViewModel.Email.Should().Be(InvalidEmail);
            }

            [Fact, LogIfTooSlow]
            public void SetsThePasswordFieldWhenValidCredentialsAreProvided()
            {
                var scheduler = new TestScheduler();
                var observable = arrangeCallToPasswordManagerWithValidCredentials();

                scheduler.Schedule(TimeSpan.FromTicks(20), () => ViewModel.StartPasswordManagerCommand.ExecuteAsync());

                scheduler.Start(
                    () => observable,
                    created: 0,
                    subscribed: 10,
                    disposed: 100
                );

                ViewModel.Password.Should().Be(ValidPassword);
            }

            [Fact, LogIfTooSlow]
            public void DoesNotSetThePasswordFieldWhenInvalidCredentialsAreProvided()
            {
                var scheduler = new TestScheduler();
                var observable = arrangeCallToPasswordManagerWithInvalidCredentials();

                scheduler.Schedule(TimeSpan.FromTicks(20), () => ViewModel.StartPasswordManagerCommand.ExecuteAsync());

                scheduler.Start(
                    () => observable,
                    created: 0,
                    subscribed: 10,
                    disposed: 100
                );

                ViewModel.Password.Should().Be(Password.Empty);
            }

            [Fact, LogIfTooSlow]
            public void DoesNothingWhenValidCredentialsAreNotProvided()
            {
                var scheduler = new TestScheduler();
                var observable = arrangeCallToPasswordManagerWithInvalidCredentials();

                scheduler.Schedule(TimeSpan.FromTicks(20), () => ViewModel.StartPasswordManagerCommand.ExecuteAsync());

                scheduler.Start(
                    () => observable,
                    created: 0,
                    subscribed: 10,
                    disposed: 100
                );

                LoginManager.DidNotReceive().Login(Arg.Any<Email>(), Arg.Any<Password>());
            }

            [Fact, LogIfTooSlow]
            public async Task TracksThePasswordManagerButtonClicked()
            {
                PasswordManagerService.IsAvailable.Returns(true);
                arrangeCallToPasswordManagerWithInvalidCredentials();

                await ViewModel.StartPasswordManagerCommand.ExecuteAsync();

                AnalyticsService.Received().TrackPasswordManagerButtonClicked();
                AnalyticsService.DidNotReceive().TrackPasswordManagerContainsValidEmail();
                AnalyticsService.DidNotReceive().TrackPasswordManagerContainsValidPassword();
            }

            [Fact, LogIfTooSlow]
            public async Task TracksThePasswordManagerContainsValidEmail()
            {
                PasswordManagerService.IsAvailable.Returns(true);
                var loginInfo = new PasswordManagerResult(ValidEmail, InvalidPassword);
                var observable = Observable.Return(loginInfo);
                PasswordManagerService.GetLoginInformation().Returns(observable);

                await ViewModel.StartPasswordManagerCommand.ExecuteAsync();

                AnalyticsService.Received().TrackPasswordManagerButtonClicked();
                AnalyticsService.Received().TrackPasswordManagerContainsValidEmail();
                AnalyticsService.DidNotReceive().TrackPasswordManagerContainsValidPassword();
            }

            [Fact, LogIfTooSlow]
            public async Task TracksThePasswordManagerContainsValidPassword()
            {
                PasswordManagerService.IsAvailable.Returns(true);
                arrangeCallToPasswordManagerWithValidCredentials();

                await ViewModel.StartPasswordManagerCommand.ExecuteAsync();

                AnalyticsService.Received().TrackPasswordManagerButtonClicked();
                AnalyticsService.Received().TrackPasswordManagerContainsValidEmail();
                AnalyticsService.Received().TrackPasswordManagerContainsValidPassword();
            }

            private IObservable<PasswordManagerResult> arrangeCallToPasswordManagerWithValidCredentials()
            {
                var loginInfo = new PasswordManagerResult(ValidEmail, ValidPassword);
                var observable = Observable.Return(loginInfo);
                PasswordManagerService.GetLoginInformation().Returns(observable);

                return observable;
            }

            private IObservable<PasswordManagerResult> arrangeCallToPasswordManagerWithInvalidCredentials()
            {
                var loginInfo = new PasswordManagerResult(InvalidEmail, InvalidPassword);
                var observable = Observable.Return(loginInfo);
                PasswordManagerService.GetLoginInformation().Returns(observable);

                return observable;
            }
        }

        public sealed class TheSignupCommand : NewLoginViewModelTest
        {
            [Property]
            public void NavigatesToTheSignupViewModel(
                NonEmptyString email, NonEmptyString password)
            {
                ViewModel.Email = Email.From(email.Get);
                ViewModel.Password = Password.From(password.Get);

                ViewModel.SignupCommand.ExecuteAsync().Wait();

                NavigationService
                    .Received()
                    .Navigate<SignupViewModel, CredentialsParameter>(
                        Arg.Is<CredentialsParameter>(parameter
                            => parameter.Email.Equals(ViewModel.Email)
                                && parameter.Password.Equals(ViewModel.Password)
                        )
                    );
            }
        }

        public sealed class ThePrepareMethod : NewLoginViewModelTest
        {
            [Property]
            public void SetsTheEmail(NonEmptyString emailString)
            {
                var email = Email.From(emailString.Get);
                var password = Password.Empty;
                var parameter = CredentialsParameter.With(email, password);

                ViewModel.Prepare(parameter);

                ViewModel.Email.Should().Be(email);
            }

            [Property]
            public void SetsThePassword(NonEmptyString passwordString)
            {
                var email = Email.Empty;
                var password = Password.From(passwordString.Get);
                var parameter = CredentialsParameter.With(email, password);

                ViewModel.Prepare(parameter);

                ViewModel.Password.Should().Be(password);
            }
        }
    }
}
