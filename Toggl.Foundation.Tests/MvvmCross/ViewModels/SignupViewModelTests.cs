using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Tests.Generators;
using Toggl.Multivac;
using Toggl.Ultrawave.Exceptions;
using Toggl.Ultrawave.Network;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class SignupViewModelTests
    {
        public abstract class SignupViewModelTest : BaseViewModelTests<SignupViewModel>
        {
            protected readonly Email ValidEmail = Email.From("susancalvin@psychohistorian.museum");
            protected readonly Email InvalidEmail = Email.From("foo@");

            protected readonly Password ValidPassword = Password.From("123456");
            protected readonly Password InvalidPassword = Password.Empty;

            protected override SignupViewModel CreateViewModel()
                => new SignupViewModel(
                    LoginManager,
                    AnalyticsService,
                    OnboardingStorage,
                    NavigationService,
                    ApiErrorHandlingService);
        }

        public sealed class TheConstructor : SignupViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(FiveParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool userLoginManager,
                bool useAnalyticsService,
                bool useOnboardingStorage,
                bool userNavigationService,
                bool useApiErrorHandlingService)
            {
                var loginManager = userLoginManager ? LoginManager : null;
                var analyticsSerivce = useAnalyticsService ? AnalyticsService : null;
                var onboardingStorage = useOnboardingStorage ? OnboardingStorage : null;
                var navigationService = userNavigationService ? NavigationService : null;
                var apiErrorHandlingService = useApiErrorHandlingService ? ApiErrorHandlingService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new SignupViewModel(
                        loginManager,
                        analyticsSerivce,
                        onboardingStorage,
                        navigationService,
                        apiErrorHandlingService);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheGoogleSignupCommand : SignupViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void CallsTheLoginManager()
            {
                ViewModel.GoogleSignupCommand.Execute();

                LoginManager.Received().SignUpWithGoogle();
            }

            [Fact, LogIfTooSlow]
            public void DoesNothingWhenThePageIsCurrentlyLoading()
            {
                var never = Observable.Never<ITogglDataSource>();
                LoginManager.SignUpWithGoogle().Returns(never);
                ViewModel.GoogleSignupCommand.Execute();

                ViewModel.GoogleSignupCommand.Execute();

                LoginManager.Received(1).SignUpWithGoogle();
            }

            [Fact, LogIfTooSlow]
            public void SetsTheIsLoadingPropertyToTrue()
            {
                LoginManager.SignUpWithGoogle().Returns(
                    Observable.Never<ITogglDataSource>());

                ViewModel.GoogleSignupCommand.Execute();

                ViewModel.IsLoading.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public void TracksGoogleSignupEvent()
            {
                LoginManager.SignUpWithGoogle().Returns(
                    Observable.Return(Substitute.For<ITogglDataSource>()));

                ViewModel.GoogleSignupCommand.Execute();

                AnalyticsService.Received().TrackSignUpEvent(AuthenticationMethod.Google);
            }

            [Fact, LogIfTooSlow]
            public void StopsTheViewModelLoadStateWhenItErrors()
            {
                LoginManager.SignUpWithGoogle().Returns(
                    Observable.Throw<ITogglDataSource>(new GoogleLoginException(false)));

                ViewModel.GoogleSignupCommand.Execute();

                ViewModel.IsLoading.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public void DoesNotNavigateWhenTheLoginFails()
            {
                LoginManager.SignUpWithGoogle().Returns(
                    Observable.Throw<ITogglDataSource>(new GoogleLoginException(false)));

                ViewModel.GoogleSignupCommand.Execute();

                NavigationService.DidNotReceive().Navigate<MainViewModel>();
            }

            [Fact, LogIfTooSlow]
            public void DoesNotDisplayAnErrormessageWhenTheUserCancelsTheRequestOnTheGoogleService()
            {
                LoginManager.SignUpWithGoogle().Returns(
                    Observable.Throw<ITogglDataSource>(new GoogleLoginException(true)));

                ViewModel.GoogleSignupCommand.Execute();

                ViewModel.HasError.Should().BeFalse();
                ViewModel.ErrorText.Should().BeEmpty();
            }

            public sealed class WhenSignupSucceeds : SuccessfulSignupTest
            {
                protected override void AdditionalViewModelSetup()
                {
                    LoginManager
                        .SignUpWithGoogle()
                        .Returns(Observable.Return(DataSource));

                    ViewModel.GoogleSignupCommand.Execute();
                }
            }
        }

        public sealed class TheTogglePasswordVisibilityCommand : SignupViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void SetsIsPasswordMaskedToFalseWhenItIsTrue()
            {
                ViewModel.TogglePasswordVisibilityCommand.Execute();

                ViewModel.IsPasswordMasked.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public void SetsIsPasswordMaskedToTrueWhenItIsFalse()
            {
                ViewModel.TogglePasswordVisibilityCommand.Execute();

                ViewModel.TogglePasswordVisibilityCommand.Execute();

                ViewModel.IsPasswordMasked.Should().BeTrue();
            }
        }

        public sealed class TheLoginCommand : SignupViewModelTest
        {
            [Property]
            public void NavigatesToLoginViewModel(
                NonEmptyString email, NonEmptyString password)
            {
                ViewModel.Email = Email.From(email.Get);
                ViewModel.Password = Password.From(password.Get);

                ViewModel.LoginCommand.Execute();

                NavigationService
                    .Received()
                    .Navigate<NewLoginViewModel, CredentialsParameter>(
                        Arg.Is<CredentialsParameter>(parameter
                            => parameter.Email.Equals(ViewModel.Email)
                                && parameter.Password.Equals(ViewModel.Password)
                        )
                    );
            }

            [Fact, LogIfTooSlow]
            public void PassesTheCredentialsToLoginViewModelIfUserTriedToSignUpWithExistingEmail()
            {
                var exception = new EmailIsAlreadyUsedException(
                    new BadRequestException(
                        Substitute.For<IRequest>(), Substitute.For<IResponse>()
                    )
                );
                LoginManager
                    .SignUp(Arg.Any<Email>(), Arg.Any<Password>(), true, Arg.Any<int>())
                    .Returns(
                        Observable.Throw<ITogglDataSource>(exception)
                    );
                NavigationService.Navigate<bool>(typeof(TermsOfServiceViewModel)).Returns(true);
                ViewModel.Email = ValidEmail;
                ViewModel.Password = ValidPassword;
                ViewModel.SignupCommand.Execute();

                ViewModel.LoginCommand.Execute();

                NavigationService
                    .Received()
                    .Navigate<NewLoginViewModel, CredentialsParameter>(
                        Arg.Is<CredentialsParameter>(
                            parameter => parameter.Email.Equals(ViewModel.Email)
                                && parameter.Password.Equals(ViewModel.Password)
                        )
                    );
            }
        }

        public sealed class TheSignupCommand : SignupViewModelTest
        {
            protected override void AdditionalViewModelSetup()
            {
                ViewModel.Email = ValidEmail;
                ViewModel.Password = ValidPassword;
            }

            [Fact, LogIfTooSlow]
            public async Task NavigatesToTheTermsOfServiceViewModel()
            {
                await ViewModel.SignupCommand.ExecuteAsync();

                await NavigationService.Received().Navigate<bool>(typeof(TermsOfServiceViewModel));
            }

            [Fact, LogIfTooSlow]
            public void DoesNotTryToSignUpIfUserDoesNotAcceptTermsOfService()
            {
                NavigationService
                    .Navigate<bool>(typeof(TermsOfServiceViewModel))
                    .Returns(false);

                ViewModel.SignupCommand.Execute();

                LoginManager.DidNotReceive().SignUp(
                    Arg.Any<Email>(),
                    Arg.Any<Password>(),
                    Arg.Any<bool>(),
                    Arg.Any<int>());
            }

            [Fact, LogIfTooSlow]
            public void ShowsTheTermsOfServiceViewModelOnlyOnceIfUserAcceptsTheTerms()
            {
                NavigationService
                    .Navigate<bool>(typeof(TermsOfServiceViewModel))
                    .Returns(true);
                LoginManager
                    .SignUp(Arg.Any<Email>(), Arg.Any<Password>(), Arg.Any<bool>(), Arg.Any<int>())
                    .Returns(Observable.Throw<ITogglDataSource>(new Exception()));

                ViewModel.SignupCommand.Execute();
                ViewModel.SignupCommand.Execute();

                NavigationService.Received(1).Navigate<bool>(typeof(TermsOfServiceViewModel));
            }

            public sealed class WhenUserAcceptsTheTermsOfService : SignupViewModelTest
            {
                protected override void AdditionalSetup()
                {
                    NavigationService
                        .Navigate<bool>(typeof(TermsOfServiceViewModel))
                        .Returns(true);
                }

                protected override void AdditionalViewModelSetup()
                {
                    ViewModel.Email = ValidEmail;
                    ViewModel.Password = ValidPassword;
                    ViewModel.SignupCommand.Execute();
                }

                [Fact, LogIfTooSlow]
                public void SetsIsLoadingToTrue()
                {
                    ViewModel.IsLoading.Should().BeTrue();
                }

                [Fact, LogIfTooSlow]
                public void TriesToSignUp()
                {
                    LoginManager.Received().SignUp(
                        ViewModel.Email,
                        ViewModel.Password,
                        true,
                        (int)ViewModel.Country.Id);
                }

                [Fact, LogIfTooSlow]
                public void TracksSignupEvent()
                {
                    AnalyticsService
                        .Received()
                        .TrackSignUpEvent(AuthenticationMethod.EmailAndPassword);
                }

                public sealed class WhenSignupSucceeds : SuccessfulSignupTest
                {
                    protected override void AdditionalViewModelSetup()
                    {
                        ViewModel.Email = ValidEmail;
                        ViewModel.Password = ValidPassword;
                        LoginManager
                            .SignUp(Arg.Any<Email>(), Arg.Any<Password>(), Arg.Any<bool>(), Arg.Any<int>())
                            .Returns(Observable.Return(DataSource));
                        NavigationService
                            .Navigate<bool>(typeof(TermsOfServiceViewModel))
                            .Returns(true);
                        ViewModel.SignupCommand.Execute();
                    }
                }

                public sealed class WhenSignupFails : SignupViewModelTest
                {
                    private void prepareException(Exception exception)
                    {
                        LoginManager
                            .SignUp(Arg.Any<Email>(), Arg.Any<Password>(), Arg.Any<bool>(), Arg.Any<int>())
                            .Returns(Observable.Throw<ITogglDataSource>(exception));
                    }

                    protected override void AdditionalViewModelSetup()
                    {
                        ViewModel.Email = ValidEmail;
                        ViewModel.Password = ValidPassword;

                        NavigationService
                            .Navigate<bool>(typeof(TermsOfServiceViewModel))
                            .Returns(true);
                    }

                    [Fact, LogIfTooSlow]
                    public void SetsIsLoadingToFalse()
                    {
                        prepareException(new Exception());

                        ViewModel.SignupCommand.Execute();

                        ViewModel.IsLoading.Should().BeFalse();
                    }

                    [Fact, LogIfTooSlow]
                    public void SetsIncorrectEmailOrPasswordErrorIfReceivedUnautghorizedException()
                    {
                        prepareException(new UnauthorizedException(
                            Substitute.For<IRequest>(),
                            Substitute.For<IResponse>()));

                        ViewModel.SignupCommand.Execute();

                        ViewModel.ErrorText.Should().Be(Resources.IncorrectEmailOrPassword);
                    }

                    [Fact, LogIfTooSlow]
                    public void SetsEmailAlreadyUsedErrorIfReceivedEmailIsAlreadyusedException()
                    {
                        prepareException(new EmailIsAlreadyUsedException(
                            new BadRequestException(
                                Substitute.For<IRequest>(),
                                Substitute.For<IResponse>()
                            )
                        ));

                        ViewModel.SignupCommand.Execute();

                        ViewModel.ErrorText.Should().Be(Resources.EmailIsAlreadyUsedError);
                    }

                    [Fact, LogIfTooSlow]
                    public void SetsGenereicErrorForAnyOtherException()
                    {
                        prepareException(new Exception());

                        ViewModel.SignupCommand.Execute();

                        ViewModel.ErrorText.Should().Be(Resources.GenericSignUpError);
                    }
                }
            }
        }

        public abstract class SuccessfulSignupTest : SignupViewModelTest
        {
            protected abstract override void AdditionalViewModelSetup();

            [Fact, LogIfTooSlow]
            public void StartsSyncing()
            {
                DataSource.Received().StartSyncing();
            }

            [Fact, LogIfTooSlow]
            public void SetsIsNewUserToTrue()
            {
                OnboardingStorage.Received().SetIsNewUser(true);
            }

            [Fact, LogIfTooSlow]
            public void SetsUserSignedUp()
            {
                OnboardingStorage.Received().SetUserSignedUp();
            }

            [Fact, LogIfTooSlow]
            public void NavigatesToMainViewModel()
            {
                NavigationService.Received().Navigate<MainViewModel>();
            }
        }

        public sealed class TheSignupEnabledProperty : SignupViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void ReturnsTrueWhenEmailAndPasswordAreValidAndIsNotLoading()
            {
                ViewModel.Email = ValidEmail;
                ViewModel.Password = ValidPassword;

                ViewModel.SignupEnabled.Should().BeTrue();
            }

            [Theory]
            [InlineData("not an email", "123")]
            [InlineData("not an email", "1234567")]
            [InlineData("this@is.email", "123")]
            public void ReturnsFalseWhenEmailOrPasswordIsInvalid(string email, string password)
            {
                ViewModel.Email = Email.From(email);
                ViewModel.Password = Password.From(password);

                ViewModel.SignupEnabled.Should().BeFalse();
            }

            [Fact]
            public void ReturnsFlaseWhenIsLoading()
            {
                ViewModel.Email = ValidEmail;
                ViewModel.Password = ValidPassword;
                NavigationService
                    .Navigate<bool>(typeof(TermsOfServiceViewModel))
                    .Returns(true);
                LoginManager
                    .SignUp(Arg.Any<Email>(), Arg.Any<Password>(), Arg.Any<bool>(), Arg.Any<int>())
                    .Returns(Observable.Never<ITogglDataSource>());
                ViewModel.SignupCommand.Execute();

                ViewModel.SignupEnabled.Should().BeFalse();
            }
        }

        public sealed class ThePrepareCommand : SignupViewModelTest
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
