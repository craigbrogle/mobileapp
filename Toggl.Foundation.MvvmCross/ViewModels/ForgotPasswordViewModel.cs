using System;
using System.Reactive.Linq;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using PropertyChanged;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Login;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class ForgotPasswordViewModel : MvxViewModel<EmailParameter, EmailParameter>
    {
        private readonly ITimeService timeService;
        private readonly ILoginManager loginManager;
        private readonly IAnalyticsService analyticsService;
        private readonly IMvxNavigationService navigationService;

        private readonly TimeSpan delayAfterPassordReset = TimeSpan.FromSeconds(4);

        public Email Email { get; set; } = Email.Empty;

        public string ErrorMessage { get; private set; }

        [DependsOn(nameof(ErrorMessage))]
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public bool IsLoading { get; private set; }

        public bool PasswordResetSuccessful { get; private set; }

        public IMvxCommand ResetCommand { get; }

        public IMvxCommand CloseCommand { get; }

        public ForgotPasswordViewModel(
            ITimeService timeService,
            ILoginManager loginManager,
            IAnalyticsService analyticsService,
            IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(loginManager, nameof(loginManager));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.timeService = timeService;
            this.loginManager = loginManager;
            this.analyticsService = analyticsService;
            this.navigationService = navigationService;

            ResetCommand = new MvxCommand(reset, () => Email.IsValid && !IsLoading);
            CloseCommand = new MvxCommand(returnEmail);
        }

        public override void Prepare(EmailParameter parameter)
        {
            Email = parameter.Email;
        }

        private void reset()
        {
            ErrorMessage = "";
            IsLoading = true;
            PasswordResetSuccessful = false;

            loginManager
                .ResetPassword(Email)
                .Do(_ => analyticsService.TrackResetPassword())
                .Subscribe(onPasswordResetSuccess, onPasswordResetError);
        }

        private void OnEmailChanged()
        {
            ResetCommand.RaiseCanExecuteChanged();
        }

        private void OnIsLoadingChanged()
        {
            ResetCommand.RaiseCanExecuteChanged();
        }

        private void onPasswordResetSuccess(string result)
        {
            IsLoading = false;
            PasswordResetSuccessful = true;

            timeService.RunAfterDelay(delayAfterPassordReset, returnEmail);
        }

        private void returnEmail()
        {
            navigationService.Close(this, EmailParameter.With(Email));
        }

        private void onPasswordResetError(Exception exception)
        {
            IsLoading = false;

            switch (exception)
            {
                case BadRequestException _:
                    ErrorMessage = Resources.PasswordResetEmailDoesNotExistError;
                    break;

                case OfflineException _:
                    ErrorMessage = Resources.PasswordResetOfflineError;
                    break;

                case ApiException apiException:
                    ErrorMessage = apiException.LocalizedApiErrorMessage;
                    break;

                default:
                    ErrorMessage = Resources.PasswordResetGeneralError;
                    break;
            }
        }
    }
}
