using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using PropertyChanged;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.Login;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.Services;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class LoginViewModel : MvxViewModel<LoginType>
    {
        public const int EmailPage = 0;
        public const int PasswordPage = 1;
        public const int ForgotPasswordPage = 2;

        private readonly ILoginManager loginManager;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IMvxNavigationService navigationService;
        private readonly IPasswordManagerService passwordManagerService;
        private readonly IApiErrorHandlingService apiErrorHandlingService;
        private readonly IAnalyticsService analyticsService;

        private LoginType loginType;
        private IDisposable loginDisposable;
        private bool tryLoggingInInstead;

        private int pageBeforeForgotPasswordPage;

        public bool IsLogin => loginType == LoginType.Login;

        public bool IsSignUp => loginType == LoginType.SignUp;

        [DependsOn(nameof(IsLogin), nameof(IsForgotPasswordPage))]
        public string Title
        {
            get
            {
                if (IsSignUp)
                    return Resources.SignUpTitle;

                if (IsForgotPasswordPage)
                    return Resources.LoginForgotPassword;

                return Resources.LoginTitle;
            }
        }

        public Email Email { get; set; } = Email.Empty;

        public Password Password { get; set; } = Password.Empty;

        public string InfoText { get; set; } = "";

        [DependsOn(nameof(IsSignUp))]
        public bool TryLoggingInInsteadOfSignup => IsSignUp && tryLoggingInInstead;

        [DependsOn(nameof(InfoText))]
        public bool HasInfoText => !string.IsNullOrEmpty(InfoText);

        [DependsOn(nameof(InfoText))]
        public bool IsErrorText { get; private set; }

        public int CurrentPage { get; private set; } = EmailPage;

        public bool IsLoading { get; private set; } = false;

        public bool IsPasswordMasked { get; private set; } = true;

        public bool PasswordManagerVisible
            => IsPasswordManagerAvailable && IsEmailPage && !IsLoading;

        public bool IsEmailFocused
            => CurrentPage == EmailPage || CurrentPage == ForgotPasswordPage;

        public IMvxCommand NextCommand { get; }

        public IMvxCommand BackCommand { get; }

        public IMvxCommand GoogleLoginCommand { get; }

        public IMvxCommand ForgotPasswordCommand { get; }

        public IMvxCommand OpenPrivacyPolicyCommand { get; }

        public IMvxCommand OpenTermsOfServiceCommand { get; }

        public IMvxCommand TogglePasswordVisibilityCommand { get; }

        public IMvxAsyncCommand StartPasswordManagerCommand { get; }

        public IMvxCommand ChangeSignUpToLoginCommand { get; }

        [DependsOn(nameof(CurrentPage))]
        public bool IsEmailPage => CurrentPage == EmailPage;

        [DependsOn(nameof(CurrentPage))]
        public bool IsPasswordPage => CurrentPage == PasswordPage;

        [DependsOn(nameof(CurrentPage))]
        public bool IsForgotPasswordPage => CurrentPage == ForgotPasswordPage;

        [DependsOn(nameof(IsEmailPage), nameof(IsForgotPasswordPage))]
        public bool EmailFieldVisible => IsEmailPage || IsForgotPasswordPage;

        [DependsOn(nameof(IsPasswordPage), nameof(IsLoading))]
        public bool ShowPasswordButtonVisible => IsPasswordPage && !IsLoading;

        [DependsOn(nameof(IsLogin), nameof(IsForgotPasswordPage))]
        public bool ShowForgotPassword => IsLogin && !IsForgotPasswordPage;

        [DependsOn(nameof(IsLogin))]
        public string GoogleButtonText => IsLogin ? Resources.GoogleLogin : Resources.GoogleSignUp;

        [DependsOn(nameof(CurrentPage), nameof(Password), nameof(Email))]
        public bool NextIsEnabled
        {
            get
            {
                if (IsEmailPage)
                    return Email.IsValid;
                if (IsPasswordPage)
                    return Password.IsValid && !IsLoading;
                if (IsForgotPasswordPage)
                    return Email.IsValid && !IsLoading;
                return false;
            }
        }

        public bool IsPasswordManagerAvailable
            => passwordManagerService.IsAvailable;

        public LoginViewModel(
            ILoginManager loginManager,
            IOnboardingStorage onboardingStorage,
            IMvxNavigationService navigationService,
            IPasswordManagerService passwordManagerService,
            IApiErrorHandlingService apiErrorHandlingService,
            IAnalyticsService analyticsService)
        {
            Ensure.Argument.IsNotNull(loginManager, nameof(loginManager));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(passwordManagerService, nameof(passwordManagerService));
            Ensure.Argument.IsNotNull(apiErrorHandlingService, nameof(apiErrorHandlingService));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.loginManager = loginManager;
            this.onboardingStorage = onboardingStorage;
            this.navigationService = navigationService;
            this.passwordManagerService = passwordManagerService;
            this.apiErrorHandlingService = apiErrorHandlingService;
            this.analyticsService = analyticsService;

            BackCommand = new MvxCommand(back);
            NextCommand = new MvxCommand(next, () => NextIsEnabled);
            GoogleLoginCommand = new MvxCommand(googleLogin);
            ForgotPasswordCommand = new MvxCommand(forgotPassword);
            OpenPrivacyPolicyCommand = new MvxCommand(openPrivacyPolicyCommand);
            OpenTermsOfServiceCommand = new MvxCommand(openTermsOfServiceCommand);
            StartPasswordManagerCommand = new MvxAsyncCommand(startPasswordManager, () => IsPasswordManagerAvailable);
            TogglePasswordVisibilityCommand = new MvxCommand(togglePasswordVisibility);
            ChangeSignUpToLoginCommand = new MvxCommand(changeSignUpToLogin, () => IsSignUp);
        }

        public override void Prepare(LoginType parameter)
        {
            loginType = parameter;
        }

        private void OnEmailChanged()
        {
            //Needed for Android
            NextCommand.RaiseCanExecuteChanged();
        }

        private void OnPasswordChanged()
        {
            if (IsSignUp)
            {
                validatePassword();
            }
        }

        private void openTermsOfServiceCommand() =>
            navigationService.Navigate<BrowserViewModel, BrowserParameters>(
                BrowserParameters.WithUrlAndTitle(Resources.TermsOfServiceUrl, Resources.TermsOfService)
            );

        private void openPrivacyPolicyCommand() =>
            navigationService.Navigate<BrowserViewModel, BrowserParameters>(
                BrowserParameters.WithUrlAndTitle(Resources.PrivacyPolicyUrl, Resources.PrivacyPolicy)
            );

        private void next()
        {
            if (!NextIsEnabled) return;

            tryLoggingInInstead = false;
            RaisePropertyChanged(nameof(TryLoggingInInsteadOfSignup));

            if (IsPasswordPage)
            {
                if (IsLogin) login();
                if (IsSignUp) signUp();
            }

            if (IsForgotPasswordPage)
            {
                resetPassword();
                return;
            }

            CurrentPage = PasswordPage;
            if (IsSignUp)
            {
                validatePassword();
            }
        }

        private void validatePassword()
        {
            IsErrorText = true;

            InfoText = Password.IsValid
                ? String.Empty
                : Resources.SignUpPasswordRequirements;

            RaisePropertyChanged(nameof(InfoText));
        }

        private void resetPassword()
        {
            IsLoading = true;
            loginManager
                .ResetPassword(Email)
                .Do(_ => analyticsService.TrackResetPassword())
                .Subscribe(onPasswordResetSuccess, onPasswordResetError);
        }

        private void onPasswordResetSuccess(string result)
        {
            IsLoading = false;
            CurrentPage = PasswordPage;
            IsErrorText = false;
            InfoText = Resources.PasswordResetSuccess;
        }

        private void onPasswordResetError(Exception exception)
        {
            IsLoading = false;

            IsErrorText = true;

            switch (exception)
            {
                case BadRequestException _:
                    InfoText = Resources.PasswordResetEmailDoesNotExistError;
                    break;

                case OfflineException _:
                    InfoText = Resources.PasswordResetOfflineError;
                    break;

                case ApiException apiException:
                    InfoText = apiException.LocalizedApiErrorMessage;
                    break;

                default:
                    InfoText = Resources.PasswordResetGeneralError;
                    break;
            }
        }

        private void back()
        {
            if (IsEmailPage)
                navigationService.Close(this);

            if (IsForgotPasswordPage)
                CurrentPage = pageBeforeForgotPasswordPage;
            else
                CurrentPage--;

            InfoText = "";
            tryLoggingInInstead = false;
            RaisePropertyChanged(nameof(TryLoggingInInsteadOfSignup));
        }

        private void togglePasswordVisibility()
            => IsPasswordMasked = !IsPasswordMasked;

        private void login()
        {
            IsLoading = true;

            loginDisposable =
                loginManager
                    .Login(Email, Password)
                    .Do(_ => analyticsService.TrackLoginEvent(AuthenticationMethod.EmailAndPassword))
                    .Subscribe(onDataSource, onError, onCompleted);
        }

        private void googleLogin()
        {
            if (IsLoading) return;

            IsLoading = true;

            var googleObservable = IsLogin
                ? loginManager
                    .LoginWithGoogle()
                    .Do(_ => analyticsService.TrackLoginEvent(AuthenticationMethod.Google))
                : loginManager
                    .SignUpWithGoogle()
                    .Do(_ => analyticsService.TrackSignUpEvent(AuthenticationMethod.Google));
            loginDisposable = googleObservable.Subscribe(onDataSource, onError, onCompleted);
        }

        private void signUp()
        {
            IsLoading = true;

            var termsAccepted = true;
            int? countryId = null;

            loginDisposable =
                loginManager
                    .SignUp(Email, Password, termsAccepted, countryId)
                    .Do(_ => analyticsService.TrackSignUpEvent(AuthenticationMethod.EmailAndPassword))
                    .Subscribe(onDataSource, onError, onCompleted);
        }

        private async Task startPasswordManager()
        {
            analyticsService.TrackPasswordManagerButtonClicked();

            var loginInfo = await passwordManagerService.GetLoginInformation();

            Email = loginInfo.Email;
            if (!NextIsEnabled) return;

            analyticsService.TrackPasswordManagerContainsValidEmail();

            next();
            Password = loginInfo.Password;
            if (!NextIsEnabled) return;

            analyticsService.TrackPasswordManagerContainsValidPassword();

            next();
        }

        private async void onDataSource(ITogglDataSource dataSource)
        {
            await dataSource.StartSyncing();

            IsLoading = false;

            onboardingStorage.SetIsNewUser(IsSignUp);
            if (IsSignUp)
            {
                onboardingStorage.SetUserSignedUp();
            }

            await navigationService.Navigate<MainViewModel>();
        }

        private void onError(Exception exception)
        {
            IsLoading = false;
            onCompleted();

            if (apiErrorHandlingService.TryHandleDeprecationError(exception))
                return;

            IsErrorText = true;

            switch (exception)
            {
                case UnauthorizedException forbidden:
                    InfoText = Resources.IncorrectEmailOrPassword;
                    break;
                case GoogleLoginException googleEx when googleEx.LoginWasCanceled:
                    InfoText = "";
                    break;
                case EmailIsAlreadyUsedException _ when IsSignUp:
                    InfoText = Resources.EmailIsAlreadyUsedError;
                    tryLoggingInInstead = true;
                    RaisePropertyChanged(nameof(TryLoggingInInsteadOfSignup));
                    break;
                default:
                    InfoText = getGenericError();
                    break;
            }

            if (IsSignUp)
            {
                analyticsService.TrackSignUpErrorEvent(signUpErrorSource(exception));
            }
            else
            {
                analyticsService.TrackLoginErrorEvent(loginErrorSource(exception));
            }
        }

        private LoginErrorSource loginErrorSource(Exception exception)
        {
            switch (exception)
            {
                case UnauthorizedException _:
                    return LoginErrorSource.InvalidEmailOrPassword;
                case GoogleLoginException _:
                    return LoginErrorSource.GoogleLoginError;
                case OfflineException _:
                    return LoginErrorSource.Offline;
                case ServerErrorException _:
                    return LoginErrorSource.ServerError;
                default:
                    return LoginErrorSource.Other;
            }
        }

        private SignUpErrorSource signUpErrorSource(Exception exception)
        {
            switch (exception)
            {
                case EmailIsAlreadyUsedException _:
                    return SignUpErrorSource.EmailIsAlreadyUsed;
                case OfflineException _:
                    return SignUpErrorSource.Offline;
                case ServerErrorException _:
                    return SignUpErrorSource.ServerError;
                default:
                    return SignUpErrorSource.Other;
            }
        }

        private string getGenericError()
            => loginType == LoginType.Login ? Resources.GenericLoginError : Resources.GenericSignUpError;

        private void onCompleted()
        {
            loginDisposable?.Dispose();
            loginDisposable = null;
        }

        private void forgotPassword()
        {
            pageBeforeForgotPasswordPage = CurrentPage;
            CurrentPage = ForgotPasswordPage;
            IsErrorText = false;
            InfoText = Email.IsValid ? "" : Resources.PasswordResetExplanation;
        }

        private void changeSignUpToLogin()
        {
            tryLoggingInInstead = false;
            InfoText = String.Empty;
            loginType = LoginType.Login;
            Password = Password.Empty;
            CurrentPage = EmailPage;
            RaisePropertyChanged(nameof(TryLoggingInInsteadOfSignup));
        }

        public override void ViewDestroy()
        {
            base.ViewDestroy();
            loginDisposable?.Dispose();
        }
    }
}
