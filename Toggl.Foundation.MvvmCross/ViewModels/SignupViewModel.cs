using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.Login;
using Toggl.Foundation.Models;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.Services;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SignupViewModel : MvxViewModel<CredentialsParameter>
    {
        private readonly ILoginManager loginManager;
        private readonly IAnalyticsService analyticsService;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IMvxNavigationService navigationService;
        private readonly IApiErrorHandlingService apiErrorHandlingService;

        private IDisposable signupDisposable;
        private bool termsOfServiceAccepted;

        public ICountry Country { get; private set; } = new Country("Estonia", "EE", 1);

        public Email Email { get; set; } = Email.Empty;

        public Password Password { get; set; } = Password.Empty;

        public bool IsLoading { get; private set; }

        public string ErrorText { get; private set; }

        public bool HasError => !string.IsNullOrEmpty(ErrorText);

        public bool IsPasswordMasked { get; private set; } = true;

        public bool SignupEnabled => Email.IsValid && Password.IsValid && !IsLoading;

        public bool IsShowPasswordButtonVisible { get; set; }

        public IMvxCommand GoogleSignupCommand { get; }

        public IMvxCommand TogglePasswordVisibilityCommand { get; }

        public IMvxAsyncCommand SignupCommand { get; }

        public IMvxAsyncCommand LoginCommand { get; }

        public IMvxAsyncCommand PickCountryCommand { get; }

        public SignupViewModel(
            ILoginManager loginManager,
            IAnalyticsService analyticsService,
            IOnboardingStorage onboardingStorage,
            IMvxNavigationService navigationService,
            IApiErrorHandlingService apiErrorHandlingService)
        {
            Ensure.Argument.IsNotNull(loginManager, nameof(loginManager));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(apiErrorHandlingService, nameof(apiErrorHandlingService));

            this.loginManager = loginManager;
            this.analyticsService = analyticsService;
            this.onboardingStorage = onboardingStorage;
            this.navigationService = navigationService;
            this.apiErrorHandlingService = apiErrorHandlingService;

            LoginCommand = new MvxAsyncCommand(login);
            GoogleSignupCommand = new MvxCommand(googleSignup);
            PickCountryCommand = new MvxAsyncCommand(pickCountry);
            SignupCommand = new MvxAsyncCommand(signup, () => SignupEnabled);
            TogglePasswordVisibilityCommand = new MvxCommand(togglePasswordVisibility);
        }


        public override void Prepare(CredentialsParameter parameter)
        {
            Email = parameter.Email;
            Password = parameter.Password;
        }

        private async Task signup()
        {
            if (!termsOfServiceAccepted)
                termsOfServiceAccepted = await navigationService.Navigate<bool>(typeof(TermsOfServiceViewModel));
            
            if (!termsOfServiceAccepted)
                return;

            IsLoading = true;

            signupDisposable =
                loginManager
                    .SignUp(Email, Password, true, (int)Country.Id)
                    .Do(_ => analyticsService.TrackSignUpEvent(AuthenticationMethod.EmailAndPassword))
                    .Subscribe(onDataSource, onError, onCompleted);
        }

        private async void onDataSource(ITogglDataSource dataSource)
        {
            await dataSource.StartSyncing();

            onboardingStorage.SetIsNewUser(true);
            onboardingStorage.SetUserSignedUp();

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
                    ErrorText = Resources.IncorrectEmailOrPassword;
                    break;
                case GoogleLoginException googleEx when googleEx.LoginWasCanceled:
                    ErrorText = "";
                    break;
                case EmailIsAlreadyUsedException _:
                    ErrorText = Resources.EmailIsAlreadyUsedError;
                    break;
                default:
                    ErrorText = Resources.GenericSignUpError;
                    break;
            }
        }

        private void onCompleted()
        {
            signupDisposable?.Dispose();
            signupDisposable = null;
        }

        private void googleSignup()
        {
            if (IsLoading) return;

            IsLoading = true;

            signupDisposable = loginManager
                .SignUpWithGoogle()
                .Do(_ => analyticsService.TrackSignUpEvent(AuthenticationMethod.Google))
                .Subscribe(onDataSource, onError, onCompleted);
        }

        private void togglePasswordVisibility()
            => IsPasswordMasked = !IsPasswordMasked;

        private async Task pickCountry()
        {
            //The dialog is not implemented yet
            await Task.CompletedTask;
        }

        private Task login()
        {
            var parameter = CredentialsParameter.With(Email, Password);
            return navigationService.Navigate<NewLoginViewModel, CredentialsParameter>(parameter);
        }

        private void OnEmailChanged()
            => SignupCommand.RaiseCanExecuteChanged();

        private void OnPasswordChanged()
            => SignupCommand.RaiseCanExecuteChanged();

        private void OnIsLoadingChanged()
            => SignupCommand.RaiseCanExecuteChanged();
    }
}
