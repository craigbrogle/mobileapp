using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Multivac;
using Toggl.Foundation.MvvmCross.Parameters;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class AboutViewModel : MvxViewModel
    {
        private readonly IMvxNavigationService navigationService;

        public AboutViewModel(IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.navigationService = navigationService;
        }

        public Task OpenPrivacyPolicyView() 
            => navigationService.Navigate<BrowserViewModel, BrowserParameters>(
                BrowserParameters.WithUrlAndTitle(Resources.PrivacyPolicyUrl, Resources.PrivacyPolicy)
            );

        public Task OpenTermsOfServiceView()
            => navigationService.Navigate<BrowserViewModel, BrowserParameters>(
                BrowserParameters.WithUrlAndTitle(Resources.TermsOfServiceUrl, Resources.TermsOfService)
            );

        public Task OpenLicensesView()
            => navigationService.Navigate<LicensesViewModel>();
    }
}
