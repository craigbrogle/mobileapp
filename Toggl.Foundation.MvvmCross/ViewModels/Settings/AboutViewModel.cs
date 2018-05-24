using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Multivac;
using Toggl.Foundation.MvvmCross.Parameters;

namespace Toggl.Foundation.MvvmCross.ViewModels.Settings
{
    [Preserve(AllMembers = true)]
    public sealed class AboutViewModel : MvxViewModel
    {
        private readonly IMvxNavigationService navigationService;
      
        public IMvxAsyncCommand OpenPrivacyPolicyCommand { get; }
        public IMvxAsyncCommand OpenTermsOfServiceCommand { get; }
        public IMvxAsyncCommand OpenLicensesCommand { get; }

        public AboutViewModel(IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.navigationService = navigationService;

            OpenPrivacyPolicyCommand = new MvxAsyncCommand(openPrivacyPolicyView);
            OpenTermsOfServiceCommand = new MvxAsyncCommand(openTermsOfServiceView);
            OpenLicensesCommand = new MvxAsyncCommand(openLicensesView);
        }


        private Task openPrivacyPolicyView() => 
            navigationService.Navigate<BrowserViewModel, BrowserParameters>(
                BrowserParameters.WithUrlAndTitle(Resources.PrivacyPolicyUrl, Resources.PrivacyPolicy)
            );

        private Task openTermsOfServiceView() => 
            navigationService.Navigate<BrowserViewModel, BrowserParameters>(
                BrowserParameters.WithUrlAndTitle(Resources.TermsOfServiceUrl, Resources.TermsOfService)
            );

        private Task openLicensesView()
            => navigationService.Navigate<LicensesViewModel>();
    }
}
