using MvvmCross.iOS.Views;
using UIKit;

namespace Toggl.Daneel.ViewControllers.Navigation
{
    public sealed class OnboardingFlowNavigationController : MvxNavigationController
    {
        public OnboardingFlowNavigationController(UIViewController viewController)
            : base(viewController)
        {
        }

        public override void PushViewController(UIViewController viewController, bool animated)
        {
            base.PushViewController(viewController, animated);

            setNavigationBarAttributes(viewController.NavigationController?.NavigationBar);
            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
        }

        public override UIViewController PopViewController(bool animated)
        {
            var viewController = base.PopViewController(animated);

            setNavigationBarAttributes(viewController.NavigationController?.NavigationBar);

            return viewController;
        }

        private static void setNavigationBarAttributes(UINavigationBar navBar)
        {
            if (navBar == null) return;

            navBar.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = UIColor.White,
                Font = UIFont.SystemFontOfSize(14, UIFontWeight.Medium)
            };
        }
    }
}
