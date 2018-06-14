using Toggl.Daneel.Extensions;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.ViewModels;

namespace Toggl.Daneel.ViewControllers
{
    public sealed partial class AboutViewController : ReactiveViewController<AboutViewModel>
    {
        public AboutViewController() 
            : base(nameof(AboutViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = Resources.About;

            this.Bind(LicensesView.Tapped(), ViewModel.OpenLicensesView);
            this.Bind(PrivacyPolicyView.Tapped(), ViewModel.OpenPrivacyPolicyView);
            this.Bind(TermsOfServiceView.Tapped(), ViewModel.OpenTermsOfServiceView);
        }
    }
}
