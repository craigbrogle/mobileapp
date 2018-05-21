using UIKit;
using System;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Multivac;

namespace Toggl.Daneel.Extensions
{
    public static class LayoutConstraintExtensions
    {
        public static void AdaptForIos10(this NSLayoutConstraint self, UINavigationBar navigationBar)
        {
            Ensure.Argument.IsNotNull(self, nameof(self));

            //on iOS 11 and later this happens automatically
            if (!UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                var statusbarHeight = UIApplication.SharedApplication.StatusBarFrame.Height;
                var navigationBarHeight = navigationBar == null ? 0 : navigationBar.Frame.Height;
                self.Constant += statusbarHeight + navigationBarHeight;
            }
        }

        public static void AnimateSetConstant(this NSLayoutConstraint self, nfloat constant, UIView containerView)
        {
            Ensure.Argument.IsNotNull(self, nameof(self));
            Ensure.Argument.IsNotNull(constant, nameof(constant));

            self.Constant = constant;

            Ensure.Argument.IsNotNull(containerView, nameof(containerView));

            UIView.Animate(Animation.Timings.EnterTiming, () => containerView.LayoutIfNeeded());
        }
    }
}
