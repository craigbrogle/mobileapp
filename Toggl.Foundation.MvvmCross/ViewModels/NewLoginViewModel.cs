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
    public sealed class NewLoginViewModel : MvxViewModel<CredentialsParameter>
    {
        private readonly ILoginManager loginManager;
        private readonly IAnalyticsService analyticsService;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IMvxNavigationService navigationService;
        private readonly IPasswordManagerService passwordManagerService;
        private readonly IApiErrorHandlingService apiErrorHandlingService;

        private IDisposable loginDisposable;

        public Email Email { get; set; } = Email.Empty;

        public Password Password { get; set; } = Password.Empty;

        public string ErrorMessage { get; private set; } = "";

        [DependsOn(nameof(ErrorMessage))]
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public bool IsLoading { get; private set; } = false;

        [DependsOn(nameof(Email), nameof(Password), nameof(IsLoading), nameof(HasError))]
        public bool LoginEnabled => Email.IsValid && Password.IsValid && !IsLoading;

        public bool IsPasswordManagerAvailable
            => passwordManagerService.IsAvailable;

        public bool IsPasswordMasked { get; private set; } = true;

        public bool IsShowPasswordButtonVisible { get; set; }

        public IMvxCommand LoginCommand { get; }

        public IMvxCommand GoogleLoginCommand { get; }

        public IMvxCommand TogglePasswordVisibilityCommand { get; }

        public IMvxAsyncCommand SignupCommand { get; }

        public IMvxAsyncCommand ForgotPasswordCommand { get; }

        public IMvxAsyncCommand StartPasswordManagerCommand { get; }

        public NewLoginViewModel(
            ILoginManager loginManager,
            IAnalyticsService analyticsService,
            IOnboardingStorage onboardingStorage,
            IMvxNavigationService navigationService,
            IPasswordManagerService passwordManagerService,
            IApiErrorHandlingService apiErrorHandlingService)
        {
            Ensure.Argument.IsNotNull(loginManager, nameof(loginManager));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(passwordManagerService, nameof(passwordManagerService));
            Ensure.Argument.IsNotNull(apiErrorHandlingService, nameof(apiErrorHandlingService));

            this.loginManager = loginManager;
            this.analyticsService = analyticsService;
            this.onboardingStorage = onboardingStorage;
            this.navigationService = navigationService;
            this.passwordManagerService = passwordManagerService;
            this.apiErrorHandlingService = apiErrorHandlingService;

            SignupCommand = new MvxAsyncCommand(signup);
            GoogleLoginCommand = new MvxCommand(googleLogin);
            LoginCommand = new MvxCommand(login, () => LoginEnabled);
            ForgotPasswordCommand = new MvxAsyncCommand(forgotPassword);
            TogglePasswordVisibilityCommand = new MvxCommand(togglePasswordVisibility);
            StartPasswordManagerCommand = new MvxAsyncCommand(startPasswordManager, () => IsPasswordManagerAvailable);
        }

        public override void Prepare(CredentialsParameter parameter)
        {
            Email = parameter.Email;
            Password = parameter.Password;
        }

        private void login()
        {
            IsLoading = true;
            ErrorMessage = "";

            loginDisposable =
                loginManager
                    .Login(Email, Password)
                    .Do(_ => analyticsService.TrackLoginEvent(AuthenticationMethod.EmailAndPassword))
                    .Subscribe(onDataSource, onError, onCompleted);
        }

        private async void onDataSource(ITogglDataSource dataSource)
        {
            await dataSource.StartSyncing();

            IsLoading = false;

            onboardingStorage.SetIsNewUser(false);

            await navigationService.Navigate<MainViewModel>();
        }

        private void onError(Exception exception)
        {
            IsLoading = false;
            onCompleted();

            if (apiErrorHandlingService.TryHandleDeprecationError(exception))
                return;

            switch (exception)
            {
                case UnauthorizedException forbidden:
                    ErrorMessage = Resources.IncorrectEmailOrPassword;
                    break;
                case GoogleLoginException googleEx when googleEx.LoginWasCanceled:
                    ErrorMessage = "";
                    break;
                default:
                    ErrorMessage = Resources.GenericLoginError;
                    break;
            }
        }

        private void onCompleted()
        {
            loginDisposable?.Dispose();
            loginDisposable = null;
        }

        private void OnEmailChanged()
        {
            LoginCommand.RaiseCanExecuteChanged();
        }

        private void OnPasswordChanged()
        {
            LoginCommand.RaiseCanExecuteChanged();
        }

        private void OnIsLoadingChanged()
        {
            LoginCommand.RaiseCanExecuteChanged();
        }

        private async Task startPasswordManager()
        {
            analyticsService.TrackPasswordManagerButtonClicked();

            var loginInfo = await passwordManagerService.GetLoginInformation();

            Email = loginInfo.Email;
            if (!Email.IsValid) return;
            analyticsService.TrackPasswordManagerContainsValidEmail();

            Password = loginInfo.Password;
            if (!Password.IsValid) return;
            analyticsService.TrackPasswordManagerContainsValidPassword();

            login();
        }

        private void togglePasswordVisibility()
           => IsPasswordMasked = !IsPasswordMasked;

        private async Task forgotPassword()
        {
            var emailParameter = EmailParameter.With(Email);
            emailParameter = await navigationService
                .Navigate<ForgotPasswordViewModel, EmailParameter, EmailParameter>(emailParameter);
            if (emailParameter != null)
                Email = emailParameter.Email;
        }

        private void googleLogin()
        {
            if (IsLoading) return;

            IsLoading = true;

            loginDisposable = loginManager
                .LoginWithGoogle()
                .Do(_ => analyticsService.TrackLoginEvent(AuthenticationMethod.Google))
                .Subscribe(onDataSource, onError, onCompleted);
        }

        private Task signup()
        {
            var parameter = CredentialsParameter.With(Email, Password);
            return navigationService.Navigate<SignupViewModel, CredentialsParameter>(parameter);
        }
    }
}
