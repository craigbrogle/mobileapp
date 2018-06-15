using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class TermsOfServiceViewModel : MvxViewModelResult<bool>
    {
        private const string privacyPolicyUrl = "https://toggl.com/legal/privacy/";
        private const string termsOfServiceUrl = "https://toggl.com/legal/terms/";

        private readonly IBrowserService browserService;
        private readonly IMvxNavigationService navigationService;

        public IMvxCommand ViewTermsOfServiceCommand { get; }

        public IMvxCommand ViewPrivacyPolicyCommand { get; }

        public IMvxAsyncCommand CloseCommand { get; }

        public IMvxAsyncCommand AcceptCommand { get; }

        public TermsOfServiceViewModel(
            IBrowserService browserService, IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(browserService, nameof(browserService));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.navigationService = navigationService;
            this.browserService = browserService;

            CloseCommand = new MvxAsyncCommand(() => close(false));
            AcceptCommand = new MvxAsyncCommand(() => close(true));
            ViewPrivacyPolicyCommand = new MvxCommand(viewPrivacyPolicy);
            ViewTermsOfServiceCommand = new MvxCommand(viewTermsOfService);
        }

        private Task close(bool accepted)
            => navigationService.Close(this, accepted);

        private void viewTermsOfService()
            => browserService.OpenUrl(termsOfServiceUrl);

        private void viewPrivacyPolicy()
            => browserService.OpenUrl(privacyPolicyUrl);
    }
}
