using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.Login;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Services;
using Toggl.Foundation.Tests.Generators;
using Toggl.Foundation.Tests.TestExtensions;
using Toggl.Multivac;
using Toggl.Ultrawave.Exceptions;
using Toggl.Ultrawave.Network;
using Xunit;
using static Toggl.Multivac.Extensions.EmailExtensions;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class LoginViewModelTests
    {
        public abstract class LoginViewModelTest : BaseViewModelTests<LoginViewModel>
        {
            protected readonly Email ValidEmail = Email.From("susancalvin@psychohistorian.museum");
            protected readonly Email InvalidEmail = Email.From("foo@");

            protected readonly Password ValidPassword = Password.From("123456");
            protected readonly Password InvalidPassword = Password.Empty;

            protected override LoginViewModel CreateViewModel()
                => new LoginViewModel(LoginManager,
                                      OnboardingStorage,
                                      NavigationService,
                                      PasswordManagerService,
                                      ApiErrorHandlingService,
                                      AnalyticsService);
        }

        public sealed class TheConstructor : LoginViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(SixParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool userLoginManager,
                bool useOnboardingStorage,
                bool userNavigationService,
                bool usePasswordManagerService,
                bool useDeprecationHandlingService,
                bool useAnalyticsService)
            {
                var loginManager = userLoginManager ? LoginManager : null;
                var onboardingStorage = useOnboardingStorage ? OnboardingStorage : null;
                var navigationService = userNavigationService ? NavigationService : null;
                var passwordManagerService = usePasswordManagerService ? PasswordManagerService : null;
                var deprecationHandlingService = useDeprecationHandlingService ? ApiErrorHandlingService : null;
                var analyticsSerivce = useAnalyticsService ? AnalyticsService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new LoginViewModel(loginManager,
                                             onboardingStorage,
                                             navigationService,
                                             passwordManagerService,
                                             deprecationHandlingService,
                                             analyticsSerivce);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheNextIsEnabledProperty
        {
            public sealed class WhenInTheEmailPage : LoginViewModelTest
            {
                [Fact, LogIfTooSlow]
                public void ReturnsFalseIfTheEmailIsInvalid()
                {
                    ViewModel.Email = InvalidEmail;

                    ViewModel.NextIsEnabled.Should().BeFalse();
                }

                [Fact, LogIfTooSlow]
                public void ReturnsTrueIfTheEmailIsValid()
                {
                    ViewModel.Email = ValidEmail;

                    ViewModel.NextIsEnabled.Should().BeTrue();
                }
            }

            public sealed class WhenInThePasswordPage : LoginViewModelTest
            {
                public WhenInThePasswordPage()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.NextCommand.Execute();
                }

                [Fact, LogIfTooSlow]
                public void ReturnsFalseWhenThePasswordIsNotValid()
                {
                    ViewModel.Password = InvalidPassword;

                    ViewModel.NextIsEnabled.Should().BeFalse();
                }

                [Fact, LogIfTooSlow]
                public void ReturnsTrueIfThePasswordIsValid()
                {
                    ViewModel.Password = ValidPassword;

                    ViewModel.NextIsEnabled.Should().BeTrue();
                }

                [Fact, LogIfTooSlow]
                public void ReturnsFalseWheThePasswordIsValidButTheViewIsLoading()
                {
                    var never = Observable.Never<ITogglDataSource>();
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>()).Returns(never);
                    ViewModel.Password = ValidPassword;
                    ViewModel.NextCommand.Execute();

                    ViewModel.NextCommand.Execute();

                    ViewModel.NextIsEnabled.Should().BeFalse();
                }
            }

            public sealed class WhenInForgotPasswordPage : LoginViewModelTest
            {
                public WhenInForgotPasswordPage()
                {
                    ViewModel.Prepare(LoginType.Login);
                    ViewModel.Email = ValidEmail;
                    ViewModel.NextCommand.Execute();
                    ViewModel.ForgotPasswordCommand.Execute();
                }

                [Fact, LogIfTooSlow]
                public void ReturnsFalseWhenTheEmailIsInvalid()
                {
                    ViewModel.Email = InvalidEmail;

                    ViewModel.NextIsEnabled.Should().BeFalse();
                }

                [Fact, LogIfTooSlow]
                public void ReturnsTrueWhenTheEmailIsValid()
                {
                    ViewModel.NextIsEnabled.Should().BeTrue();
                }

                [Fact, LogIfTooSlow]
                public void ReturnsFalseWheTheEmailIsValidButTheViewIsLoading()
                {
                    var scheduler = new TestScheduler();
                    var never = Observable.Never<string>();
                    LoginManager.ResetPassword(Arg.Any<Email>()).Returns(never);
                    ViewModel.Email = ValidEmail;

                    ViewModel.NextCommand.Execute();

                    ViewModel.NextIsEnabled.Should().BeFalse();
                }
            }
        }

        public sealed class ThePrepareMethod : LoginViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void SetsTheLoginType()
            {
                var parameter = LoginType.SignUp;

                ViewModel.Prepare(parameter);

                ViewModel.IsSignUp.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public void SetsTheTitleToLoginWhenThePassedParameterIsLogin()
            {
                var parameter = LoginType.Login;

                ViewModel.Prepare(parameter);

                ViewModel.Title.Should().Be(Resources.LoginTitle);
            }

            [Fact, LogIfTooSlow]
            public void SetsTheTitleToSignupWhenThePassedParameterIsLogin()
            {
                var parameter = LoginType.SignUp;

                ViewModel.Prepare(parameter);

                ViewModel.Title.Should().Be(Resources.SignUpTitle);
            }
        }

        public sealed class TheNextCommand
        {
            public sealed class WhenInTheEmailPage : LoginViewModelTest
            {
                [Fact, LogIfTooSlow]
                public void DoesNothingWhenTheEmailIsInvalid()
                {
                    ViewModel.Email = InvalidEmail;

                    ViewModel.NextCommand.Execute();

                    ViewModel.CurrentPage.Should().Be(LoginViewModel.EmailPage);
                }

                [Fact, LogIfTooSlow]
                public void ShowsThePasswordPageWhenTheEmailIsValid()
                {
                    ViewModel.Email = ValidEmail;

                    ViewModel.NextCommand.Execute();

                    ViewModel.CurrentPage.Should().Be(LoginViewModel.PasswordPage);
                }
            }

            public sealed class WhenInThePasswordPage : LoginViewModelTest
            {
                public WhenInThePasswordPage()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.NextCommand.Execute();
                }

                [Fact, LogIfTooSlow]
                public void DoesNotAttemptToLoginWhileThePasswordIsValid()
                {
                    ViewModel.Password = InvalidPassword;

                    ViewModel.NextCommand.Execute();

                    LoginManager.DidNotReceive().Login(Arg.Any<Email>(), Arg.Any<Password>());
                }

                [Fact, LogIfTooSlow]
                public void CallsTheLoginManagerWhenThePasswordIsValid()
                {
                    ViewModel.Password = ValidPassword;

                    ViewModel.NextCommand.Execute();

                    LoginManager.Received().Login(Arg.Any<Email>(), Arg.Any<Password>());
                }

                [Fact, LogIfTooSlow]
                public void CallsTheLoginManagerForSignUpWhenThePasswordIsValidAndInSignUpMode()
                {
                    ViewModel.Prepare(LoginType.SignUp);
                    ViewModel.Password = ValidPassword;

                    ViewModel.NextCommand.Execute();

                    LoginManager.Received().SignUp(Arg.Any<Email>(), Arg.Any<Password>(), Arg.Any<bool>(), Arg.Any<int?>());
                }

                [Fact, LogIfTooSlow]
                public void DoesNothingWhenThePageIsCurrentlyLoading()
                {
                    var scheduler = new TestScheduler();
                    var never = Observable.Never<ITogglDataSource>();
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>()).Returns(never);

                    ViewModel.Password = ValidPassword;
                    ViewModel.NextCommand.Execute();

                    ViewModel.NextCommand.Execute();

                    LoginManager.Received(1).Login(Arg.Any<Email>(), Arg.Any<Password>());
                }

                [Fact, LogIfTooSlow]
                public void NavigatesToTheTimeEntriesViewModelWhenTheLoginSucceeds()
                {
                    ViewModel.Password = ValidPassword;
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                                .Returns(Observable.Return(Substitute.For<ITogglDataSource>()));

                    ViewModel.NextCommand.Execute();

                    NavigationService.Received().Navigate<MainViewModel>();
                }

                [Fact, LogIfTooSlow]
                public void TracksTheLoginEventWhenTheLoginSucceeds()
                {
                    ViewModel.Password = ValidPassword;
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Return(Substitute.For<ITogglDataSource>()));

                    ViewModel.NextCommand.Execute();

                    AnalyticsService.Received().TrackLoginEvent(AuthenticationMethod.EmailAndPassword);
                }

                [Fact, LogIfTooSlow]
                public void TracksTheSignUpEventWhenTheSignUpSucceeds()
                {
                    ViewModel.Prepare(LoginType.SignUp);
                    ViewModel.Password = ValidPassword;

                    LoginManager.SignUp(Arg.Any<Email>(), Arg.Any<Password>(), Arg.Any<bool>(), Arg.Any<int?>())
                        .Returns(Observable.Return(Substitute.For<ITogglDataSource>()));

                    ViewModel.NextCommand.Execute();

                    AnalyticsService.Received().TrackSignUpEvent(AuthenticationMethod.EmailAndPassword);
                }

                [Fact, LogIfTooSlow]
                public void StopsTheViewModelLoadStateWhenItCompletes()
                {
                    ViewModel.Password = ValidPassword;
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                                .Returns(Observable.Return(Substitute.For<ITogglDataSource>()));

                    ViewModel.NextCommand.Execute();

                    ViewModel.IsLoading.Should().BeFalse();
                }

                [Fact, LogIfTooSlow]
                public void StopsTheViewModelLoadStateWhenItErrors()
                {
                    ViewModel.Password = ValidPassword;
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                                .Returns(Observable.Throw<ITogglDataSource>(new Exception()));

                    ViewModel.NextCommand.Execute();

                    ViewModel.IsLoading.Should().BeFalse();
                }

                [Fact, LogIfTooSlow]
                public void DoesNotNavigateWhenTheLoginFails()
                {
                    ViewModel.Password = ValidPassword;
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                                .Returns(Observable.Throw<ITogglDataSource>(new Exception()));

                    ViewModel.NextCommand.Execute();

                    NavigationService.DidNotReceive().Navigate<MainViewModel>();
                }

                [Fact, LogIfTooSlow]
                public void TracksTheLoginErrorWhenLoginFails()
                {
                    ViewModel.Password = ValidPassword;
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                                .Returns(Observable.Throw<ITogglDataSource>(new Exception()));

                    ViewModel.NextCommand.Execute();

                    AnalyticsService.Received().TrackLoginErrorEvent(LoginErrorSource.Other);
                }

                [Fact, LogIfTooSlow]
                public void TracksTheSignUpErrorWhenSignUpFails()
                {
                    ViewModel.Prepare(LoginType.SignUp);
                    ViewModel.Password = ValidPassword;

                    LoginManager.SignUp(Arg.Any<Email>(), Arg.Any<Password>(), Arg.Any<bool>(), Arg.Any<int?>())
                                .Returns(Observable.Throw<ITogglDataSource>(new Exception()));

                    ViewModel.NextCommand.Execute();

                    AnalyticsService.Received().TrackSignUpErrorEvent(SignUpErrorSource.Other);
                }
            }

            public sealed class WhenInForgotPasswordPage : LoginViewModelTest
            {
                public WhenInForgotPasswordPage()
                {
                    ViewModel.Prepare(LoginType.Login);
                    ViewModel.Email = ValidEmail;
                    ViewModel.NextCommand.Execute();
                    ViewModel.ForgotPasswordCommand.Execute();
                }

                [Fact, LogIfTooSlow]
                public void SetsTheIsLoadingFlagToTrueWhileLoginManagerIsWorking()
                {
                    var never = Observable.Never<string>();
                    LoginManager.ResetPassword(Arg.Any<Email>()).Returns(never);

                    ViewModel.NextCommand.Execute();

                    ViewModel.IsLoading.Should().BeTrue();
                }

                [Fact, LogIfTooSlow]
                public void SetsTheIsLoadingFlagToFalseWhenTheResetSucceeds()
                {
                    var observable = Observable.Return("Some api response");
                    LoginManager.ResetPassword(Arg.Any<Email>()).Returns(observable);

                    ViewModel.NextCommand.Execute();

                    ViewModel.IsLoading.Should().BeFalse();
                }

                [Fact, LogIfTooSlow]
                public void TracksTheResetPasswordEventWhenTheResetSucceeds()
                {
                    var observable = Observable.Return("Some api response");
                    LoginManager.ResetPassword(Arg.Any<Email>()).Returns(observable);

                    ViewModel.NextCommand.Execute();

                    AnalyticsService.Received().TrackResetPassword();
                }

                [Fact, LogIfTooSlow]
                public void SetsTheIsLoadingFlagToFalseWhenTheResetFails()
                {
                    var scheduler = new TestScheduler();
                    prepareException(scheduler);

                    ViewModel.NextCommand.Execute();
                    scheduler.AdvanceTo(1);

                    ViewModel.IsLoading.Should().BeFalse();
                }

                [Fact, LogIfTooSlow]
                public void SetsCurrentPageToPasswordPageWhenTheResetSucceeds()
                {
                    LoginManager.ResetPassword(Arg.Any<Email>())
                        .Returns(Observable.Return("Great success"));

                    ViewModel.NextCommand.Execute();

                    ViewModel.CurrentPage.Should().Be(LoginViewModel.PasswordPage);
                }

                [Fact, LogIfTooSlow]
                public void DoesNotUpdateCurrentPageIfTheResetFeails()
                {
                    var scheduler = new TestScheduler();
                    prepareException(scheduler);

                    ViewModel.NextCommand.Execute();
                    scheduler.AdvanceTo(1);

                    ViewModel.CurrentPage.Should().Be(LoginViewModel.ForgotPasswordPage);
                }

                private void prepareException(TestScheduler scheduler)
                {
                    var message = Notification.CreateOnError<string>(new Exception("Some api error"));
                    var recorded = new Recorded<Notification<string>>(1, message);
                    var observable = scheduler.CreateColdObservable(recorded);
                    LoginManager.ResetPassword(Arg.Any<Email>()).Returns(observable);
                }
            }
        }

        public sealed class TheGoogleLoginCommand : LoginViewModelTest
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
                var scheduler = new TestScheduler();
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
            public void TracksGoogleSignUpEvent()
            {
                ViewModel.Prepare(LoginType.SignUp);
                LoginManager.LoginWithGoogle()
                    .Returns(Observable.Return(Substitute.For<ITogglDataSource>()));

                ViewModel.GoogleLoginCommand.Execute();

                AnalyticsService.Received().TrackSignUpEvent(AuthenticationMethod.Google);
            }

            [Fact, LogIfTooSlow]
            public void TracksGoogleLoginErrorWhenLoginFails()
            {
                LoginManager.LoginWithGoogle()
                            .Returns(Observable.Throw<ITogglDataSource>(new Exception()));

                ViewModel.GoogleLoginCommand.Execute();

                AnalyticsService.Received().TrackLoginErrorEvent(LoginErrorSource.Other);
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

                ViewModel.InfoText.Should().BeEmpty();
            }
        }

        public sealed class TheTermsOfServiceCommand : LoginViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void OpensTheBrowserInTheTermsOfServicePage()
            {
                ViewModel.OpenTermsOfServiceCommand.Execute();

                NavigationService.Received().Navigate<BrowserViewModel, BrowserParameters>(
                    Arg.Is<BrowserParameters>(parameter => parameter.Url == Resources.TermsOfServiceUrl)
                );
            }

            [Fact, LogIfTooSlow]
            public void OpensTheBrowserWithTheAppropriateTitle()
            {
                ViewModel.OpenTermsOfServiceCommand.Execute();

                NavigationService.Received().Navigate<BrowserViewModel, BrowserParameters>(
                    Arg.Is<BrowserParameters>(parameter => parameter.Title == Resources.TermsOfService)
                );
            }
        }

        public sealed class ThePrivacyPolicyCommand : LoginViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void OpensTheBrowserInThePrivacyPolicyPage()
            {
                ViewModel.OpenPrivacyPolicyCommand.Execute();

                NavigationService.Received().Navigate<BrowserViewModel, BrowserParameters>(
                    Arg.Is<BrowserParameters>(parameter => parameter.Url == Resources.PrivacyPolicyUrl)
                );
            }

            [Fact, LogIfTooSlow]
            public void OpensTheBrowserWithTheAppropriateTitle()
            {
                ViewModel.OpenPrivacyPolicyCommand.Execute();

                NavigationService.Received().Navigate<BrowserViewModel, BrowserParameters>(
                    Arg.Is<BrowserParameters>(parameter => parameter.Title == Resources.PrivacyPolicy)
                );
            }
        }

        public sealed class ThePreviousCommand : LoginViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void ReturnsToTheEmailPageWhenInPasswordPage()
            {
                ViewModel.Email = ValidEmail;
                ViewModel.NextCommand.Execute();

                ViewModel.BackCommand.Execute();

                ViewModel.CurrentPage.Should().Be(LoginViewModel.EmailPage);
            }

            [Fact, LogIfTooSlow]
            public void ClosesTheViewModelWhenInTheEmailPage()
            {
                ViewModel.BackCommand.Execute();

                NavigationService.Received().Close(Arg.Is(ViewModel));
            }

            public sealed class WhenInForgotPasswordPage : LoginViewModelTest
            {
                public WhenInForgotPasswordPage()
                {
                    ViewModel.Prepare(LoginType.Login);
                }

                [Fact, LogIfTooSlow]
                public void ReturnsToEmailPageIfForgotPasswordCommandWasExecutedWhileInEmailPage()
                {
                    ViewModel.ForgotPasswordCommand.Execute();

                    ViewModel.BackCommand.Execute();

                    ViewModel.CurrentPage.Should().Be(LoginViewModel.EmailPage);
                }

                [Fact, LogIfTooSlow]
                public void ReturnsToPasswordPageIfForgotpasswordCommandWasExecutedWhileInPasswordPage()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.NextCommand.Execute();
                    ViewModel.ForgotPasswordCommand.Execute();

                    ViewModel.BackCommand.Execute();

                    ViewModel.CurrentPage.Should().Be(LoginViewModel.PasswordPage);
                }
            }
        }

        public sealed class TheStartPasswordManagerCommand : LoginViewModelTest
        {
            public TheStartPasswordManagerCommand()
            {
                PasswordManagerService.IsAvailable.Returns(true);
            }

            [Fact, LogIfTooSlow]
            public void DoesNotTryToCallThePasswordManagerServiceIfItIsNotAvailable()
            {
                PasswordManagerService.IsAvailable.Returns(false);

                ViewModel.StartPasswordManagerCommand.ExecuteAsync();

                PasswordManagerService.DidNotReceive().GetLoginInformation();
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

        public sealed class TheHasInfoTextProperty : LoginViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void IsFalseWhenLoginSucceeds()
            {
                LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                            .Returns(Observable.Return(DataSource));
                ViewModel.Email = ValidEmail;
                ViewModel.NextCommand.Execute();
                ViewModel.Password = ValidPassword;

                ViewModel.NextCommand.Execute();

                ViewModel.HasInfoText.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public void IsTrueWhenLoginFails()
            {
                var scheduler = new TestScheduler();
                var exception = new UnauthorizedException(Substitute.For<IRequest>(), Substitute.For<IResponse>());
                var notification = Notification.CreateOnError<ITogglDataSource>(exception);
                var message = new Recorded<Notification<ITogglDataSource>>(0, notification);
                var observable = scheduler.CreateColdObservable(message);
                LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                            .Returns(observable);
                ViewModel.Email = ValidEmail;
                ViewModel.NextCommand.Execute();
                ViewModel.Password = ValidPassword;

                ViewModel.NextCommand.Execute();
                scheduler.AdvanceTo(1);

                ViewModel.HasInfoText.Should().BeTrue();
            }
        }

        public sealed class TheInfoTextProperty : LoginViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void IsEmptyWhenLoginSucceeds()
            {
                LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                            .Returns(Observable.Return(DataSource));
                ViewModel.Email = ValidEmail;
                ViewModel.NextCommand.Execute();
                ViewModel.Password = ValidPassword;

                ViewModel.NextCommand.Execute();

                ViewModel.InfoText.Should().Be("");
            }

            [Fact, LogIfTooSlow]
            public void IsWrongPasswordErrorWhenUnauthorizedExceptionIsThrown()
            {
                var scheduler = new TestScheduler();
                var exception = new UnauthorizedException(Substitute.For<IRequest>(), Substitute.For<IResponse>());
                var notification = Notification.CreateOnError<ITogglDataSource>(exception);
                var message = new Recorded<Notification<ITogglDataSource>>(0, notification);
                var observable = scheduler.CreateColdObservable(message);
                LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                            .Returns(observable);
                ViewModel.Email = ValidEmail;
                ViewModel.NextCommand.Execute();
                ViewModel.Password = ValidPassword;

                ViewModel.NextCommand.Execute();
                scheduler.AdvanceTo(1);

                ViewModel.InfoText.Should().Be(Resources.IncorrectEmailOrPassword);
            }

            [Fact, LogIfTooSlow]
            public void IsGenericLoginErrorWhenAnyOtherExceptionIsThrown()
            {
                var scheduler = new TestScheduler();
                var notification = Notification.CreateOnError<ITogglDataSource>(new Exception());
                var message = new Recorded<Notification<ITogglDataSource>>(0, notification);
                var observable = scheduler.CreateColdObservable(message);
                LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                            .Returns(observable);
                ViewModel.Email = ValidEmail;
                ViewModel.NextCommand.Execute();
                ViewModel.Password = ValidPassword;

                ViewModel.NextCommand.Execute();
                scheduler.AdvanceTo(1);

                ViewModel.InfoText.Should().Be(Resources.GenericLoginError);
            }

            [Fact, LogIfTooSlow]
            public void IsPasswordRequirementsWhenSwitchingFromEmailToPasswordPageInSignUpMode()
            {
                ViewModel.Prepare(LoginType.SignUp);
                ViewModel.Email = ValidEmail;

                ViewModel.NextCommand.Execute();

                ViewModel.InfoText.Should().Be(Resources.SignUpPasswordRequirements);
            }

            [Fact, LogIfTooSlow]
            public void IsEmptyWhenSwitchingFromEmailToPasswordPageInLoginMode()
            {
                ViewModel.Prepare(LoginType.Login);
                ViewModel.Email = ValidEmail;

                ViewModel.NextCommand.Execute();

                ViewModel.InfoText.Should().Be("");
            }

            [Fact, LogIfTooSlow]
            public void IsGenericSignUpErrorWhenAnyOtherExceptionIsThrown()
            {
                var scheduler = new TestScheduler();
                var notification = Notification.CreateOnError<ITogglDataSource>(new Exception());
                var message = new Recorded<Notification<ITogglDataSource>>(0, notification);
                var observable = scheduler.CreateColdObservable(message);
                LoginManager.SignUp(Arg.Any<Email>(), Arg.Any<Password>(), Arg.Any<bool>(), Arg.Any<int?>())
                            .Returns(observable);
                ViewModel.Prepare(LoginType.SignUp);
                ViewModel.Email = ValidEmail;
                ViewModel.NextCommand.Execute();
                ViewModel.Password = ValidPassword;

                ViewModel.NextCommand.Execute();
                scheduler.AdvanceTo(1);

                ViewModel.InfoText.Should().Be(Resources.GenericSignUpError);
            }

            public sealed class WhenInForgotPasswordPage : LoginViewModelTest
            {
                public WhenInForgotPasswordPage()
                {
                    ViewModel.Prepare(LoginType.Login);
                    ViewModel.Email = ValidEmail;
                    ViewModel.NextCommand.Execute();
                }

                [Theory, LogIfTooSlow]
                [InlineData("")]
                [InlineData("something@")]
                [InlineData("something@something")]
                public void IsForgotPasswordExplanationWhenEnteredEmailisntValid(string email)
                {
                    ViewModel.Email = email.ToEmail();
                    ViewModel.ForgotPasswordCommand.Execute();

                    ViewModel.InfoText.Should().Be(Resources.PasswordResetExplanation);
                }

                [Fact, LogIfTooSlow]
                public void IsEmptyWhenEmailIsValid()
                {
                    ViewModel.ForgotPasswordCommand.Execute();

                    ViewModel.InfoText.Should().Be("");
                }

                [Fact, LogIfTooSlow]
                public void IsTheSuccessMessageIfResetSucceeds()
                {
                    ViewModel.ForgotPasswordCommand.Execute();
                    LoginManager.ResetPassword(Arg.Any<Email>())
                        .Returns(Observable.Return("Api response"));

                    ViewModel.NextCommand.Execute();

                    ViewModel.InfoText.Should().Be(Resources.PasswordResetSuccess);
                }

                [Fact, LogIfTooSlow]
                public void IsWrongEmailErrorIfLoginManagerReturnsBadRequestException()
                {
                    ViewModel.ForgotPasswordCommand.Execute();
                    var scheduler = new TestScheduler();
                    prepareException(
                        scheduler,
                        new BadRequestException(
                            Substitute.For<IRequest>(),
                            Substitute.For<IResponse>()));

                    ViewModel.NextCommand.Execute();
                    scheduler.AdvanceTo(1);

                    ViewModel.InfoText.Should()
                        .Be(Resources.PasswordResetEmailDoesNotExistError);
                }

                [Fact, LogIfTooSlow]
                public void IsOfflineErrorIfLoginManagerReturnsOfflineException()
                {
                    ViewModel.ForgotPasswordCommand.Execute();
                    var scheduler = new TestScheduler();
                    prepareException(
                        scheduler,
                        new OfflineException());

                    ViewModel.NextCommand.Execute();
                    scheduler.AdvanceTo(1);

                    ViewModel.InfoText.Should()
                        .Be(Resources.PasswordResetOfflineError);
                }

                [Fact, LogIfTooSlow]
                public void IsApiErrorMessageIfLoginManagerReturnsApiException()
                {
                    ViewModel.ForgotPasswordCommand.Execute();
                    var message = "Some api error";
                    var response = Substitute.For<IResponse>();
                    response.RawData.Returns(message);
                    var scheduler = new TestScheduler();
                    prepareException(
                        scheduler,
                        new ApiException(
                            Substitute.For<IRequest>(),
                            response,
                            message));

                    ViewModel.NextCommand.Execute();
                    scheduler.AdvanceTo(1);

                    ViewModel.InfoText.Should().Be(message);
                }

                [Fact, LogIfTooSlow]
                public void IsGeneralErrorForAnyOtherException()
                {
                    ViewModel.ForgotPasswordCommand.Execute();
                    var scheduler = new TestScheduler();
                    prepareException(
                        scheduler,
                        new Exception());

                    ViewModel.NextCommand.Execute();
                    scheduler.AdvanceTo(1);

                    ViewModel.InfoText.Should()
                        .Be(Resources.PasswordResetGeneralError);
                }

                private void prepareException(
                    TestScheduler scheduler, Exception exception)
                {
                    var notification = Notification.CreateOnError<string>(exception);
                    var message = new Recorded<Notification<string>>(1, notification);
                    var observable = scheduler.CreateColdObservable(message);
                    LoginManager.ResetPassword(Arg.Any<Email>()).Returns(observable);
                }
            }
        }

        public sealed class TheTitleProperty : LoginViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void IsSignUpTitleWhenInLoginTypeIsSignUp()
            {
                ViewModel.Prepare(LoginType.SignUp);

                ViewModel.Title.Should().Be(Resources.SignUpTitle);
            }

            public sealed class WhenInLoginTypeIsLogin : LoginViewModelTest
            {
                public WhenInLoginTypeIsLogin()
                {
                    ViewModel.Prepare(LoginType.Login);
                }

                [Fact, LogIfTooSlow]
                public void IsLoginTitleWhenInEmailPage()
                {
                    ViewModel.Title.Should().Be(Resources.LoginTitle);
                }

                [Fact, LogIfTooSlow]
                public void IsLoginTitleWhenInPasswordPage()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.NextCommand.Execute();

                    ViewModel.Title.Should().Be(Resources.LoginTitle);
                }

                [Fact, LogIfTooSlow]
                public void IsForgotPassWordTitleWhenIsForgotPasswordPage()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.NextCommand.Execute();
                    ViewModel.ForgotPasswordCommand.Execute();

                    ViewModel.Title.Should().Be(Resources.LoginForgotPassword);
                }

                [Fact, LogIfTooSlow]
                public void GetsProperlyUpdatedAfterPreviousCommand()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.NextCommand.Execute();
                    ViewModel.ForgotPasswordCommand.Execute();
                    ViewModel.BackCommand.Execute();

                    ViewModel.Title.Should().Be(Resources.LoginTitle);
                }
            }
        }

        public sealed class TheForgotPasswordCommand : LoginViewModelTest
        {
            public TheForgotPasswordCommand()
            {
                ViewModel.Prepare(LoginType.Login);
                ViewModel.Email = ValidEmail;
                ViewModel.NextCommand.Execute();
            }

            [Fact, LogIfTooSlow]
            public void ShowsTheForgotPasswordPage()
            {
                ViewModel.ForgotPasswordCommand.Execute();

                ViewModel.CurrentPage.Should().Be(LoginViewModel.ForgotPasswordPage);
                ViewModel.IsForgotPasswordPage.Should().BeTrue();
            }
        }

        public sealed class TheEmailFieldVisibleProperty : LoginViewModelTest
        {
            [Theory, LogIfTooSlow]
            [InlineData(LoginType.Login)]
            [InlineData(LoginType.SignUp)]
            public void ReturnsTrueWhenInEmailPage(LoginType loginType)
            {
                ViewModel.Prepare(loginType);

                ViewModel.EmailFieldVisible.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public void ReturnsTrueWhenInForgotPasswordPage()
            {
                ViewModel.Prepare(LoginType.Login);
                ViewModel.Email = ValidEmail;
                ViewModel.NextCommand.Execute();
                ViewModel.ForgotPasswordCommand.Execute();

                ViewModel.EmailFieldVisible.Should().BeTrue();
            }
        }

        public sealed class TheShowForgotPasswordProperty
        {
            public sealed class WhenLoginTypeIsSignUp : LoginViewModelTest
            {
                public WhenLoginTypeIsSignUp()
                {
                    ViewModel.Prepare(LoginType.SignUp);
                }

                [Fact, LogIfTooSlow]
                public void IsFlaseWhenInEmailPage()
                {
                    ViewModel.ShowForgotPassword.Should().BeFalse();
                }

                [Fact, LogIfTooSlow]
                public void IsFlaseWhenInPasswordPage()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.NextCommand.Execute();

                    ViewModel.ShowForgotPassword.Should().BeFalse();
                }
            }

            public sealed class WhenLoginTypeIsLogin : LoginViewModelTest
            {
                public WhenLoginTypeIsLogin()
                {
                    ViewModel.Prepare(LoginType.Login);
                }

                [Fact, LogIfTooSlow]
                public void IsTrueWhenInEmailPage()
                {
                    ViewModel.ShowForgotPassword.Should().BeTrue();
                }

                [Fact, LogIfTooSlow]
                public void IsTrueWhenInPasswordPage()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.NextCommand.Execute();

                    ViewModel.ShowForgotPassword.Should().BeTrue();
                }

                [Fact, LogIfTooSlow]
                public void IsFlaseWhenInForgotPasswordPage()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.ForgotPasswordCommand.Execute();

                    ViewModel.ShowForgotPassword.Should().BeFalse();
                }
            }
        }

        public sealed class TheChangeSignUpToLoginCommand : LoginViewModelTest
        {
            [Fact]
            public void CannotExecuteTheCommandWhenInLoginMode()
            {
                ViewModel.Prepare(LoginType.Login);

                var canExecute = ViewModel.ChangeSignUpToLoginCommand.CanExecute();

                canExecute.Should().BeFalse();
            }

            [Fact]
            public void CanBeExecutedWhenInSignUpMode()
            {
                ViewModel.Prepare(LoginType.SignUp);

                var canExecute = ViewModel.ChangeSignUpToLoginCommand.CanExecute();

                canExecute.Should().BeTrue();
            }

            [Fact]
            public async Task ClearsTheInfoText()
            {
                await trySignUpWithExistingEmail();

                ViewModel.ChangeSignUpToLoginCommand.Execute();

                ViewModel.InfoText.Length.Should().Be(0);
            }

            [Fact]
            public async Task ClearsThePassword()
            {
                await trySignUpWithExistingEmail();

                ViewModel.ChangeSignUpToLoginCommand.Execute();

                ViewModel.Password.Should().Be(Password.Empty);
            }

            [Fact]
            public async Task KeepsTheEmail()
            {
                await trySignUpWithExistingEmail();

                ViewModel.ChangeSignUpToLoginCommand.Execute();

                ViewModel.Email.Should().Be(ValidEmail);
            }

            [Fact]
            public async Task ChangesLoginTypeToLogin()
            {
                await trySignUpWithExistingEmail();

                ViewModel.ChangeSignUpToLoginCommand.Execute();

                ViewModel.IsLogin.Should().BeTrue();
            }

            [Fact]
            public async Task ChangesCurrentPageToEmailPage()
            {
                await trySignUpWithExistingEmail();

                ViewModel.ChangeSignUpToLoginCommand.Execute();

                ViewModel.IsEmailPage.Should().BeTrue();
            }

            [Fact]
            public async Task ClearsTheTryLoggingInInsteadOfSignupFlag()
            {
                await trySignUpWithExistingEmail();

                ViewModel.ChangeSignUpToLoginCommand.Execute();

                ViewModel.TryLoggingInInsteadOfSignup.Should().BeFalse();
            }

            private async Task trySignUpWithExistingEmail()
            {
                var request = Substitute.For<IRequest>();
                var response = Substitute.For<IResponse>();
                var badRequestException = new BadRequestException(request, response);
                var emailTakenException = new EmailIsAlreadyUsedException(badRequestException);
                LoginManager.SignUp(Arg.Any<Email>(), Arg.Any<Password>(), Arg.Any<bool>(), Arg.Any<int?>())
                    .Returns(Observable.Throw<ITogglDataSource>(emailTakenException));

                ViewModel.Prepare(LoginType.SignUp);
                await ViewModel.Initialize();

                ViewModel.Email = ValidEmail;
                ViewModel.NextCommand.Execute();
                ViewModel.Password = ValidPassword;
                ViewModel.NextCommand.Execute();
            }
        }

        public sealed class ApiErrorHandling
        {
            public abstract class BaseApiErrorHandlingTests : LoginViewModelTest
            {
                private IRequest request => Substitute.For<IRequest>();
                private IResponse response => Substitute.For<IResponse>();

                [Fact, LogIfTooSlow]
                public void HandlesTheClientDeprecatedExceptionWithTheService()
                {
                    ApiErrorHandlingService
                        .TryHandleDeprecationError(Arg.Any<ClientDeprecatedException>())
                        .Returns(true);

                    CallAndThrow(new ClientDeprecatedException(request, response));

                    ApiErrorHandlingService.Received().TryHandleDeprecationError(Arg.Any<ClientDeprecatedException>());
                }

                [Fact, LogIfTooSlow]
                public void HandlesTheApiDeprecatedExceptionWithTheService()
                {
                    ApiErrorHandlingService
                        .TryHandleDeprecationError(Arg.Any<ApiDeprecatedException>())
                        .Returns(true);

                    CallAndThrow(new ApiDeprecatedException(request, response));

                    ApiErrorHandlingService.Received().TryHandleDeprecationError(Arg.Any<ApiDeprecatedException>());
                }

                protected abstract void CallAndThrow(Exception e);
            }

            public sealed class Login : BaseApiErrorHandlingTests
            {
                protected override void CallAndThrow(Exception e)
                {
                    LoginManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(_ => Observable.Throw<ITogglDataSource>(e));

                    ViewModel.Prepare(LoginType.Login);
                    ViewModel.Email = ValidEmail;
                    ViewModel.NextCommand.Execute();
                    ViewModel.Password = ValidPassword;
                    ViewModel.NextCommand.Execute();
                }
            }

            public sealed class SignUp : BaseApiErrorHandlingTests
            {
                protected override void CallAndThrow(Exception e)
                {
                    LoginManager.SignUp(Arg.Any<Email>(), Arg.Any<Password>(), Arg.Any<bool>(), Arg.Any<int?>())
                        .Returns(_ => Observable.Throw<ITogglDataSource>(e));

                    ViewModel.Prepare(LoginType.SignUp);
                    ViewModel.Email = ValidEmail;
                    ViewModel.NextCommand.Execute();
                    ViewModel.Password = ValidPassword;
                    ViewModel.NextCommand.Execute();
                }
            }
        }
    }
}
