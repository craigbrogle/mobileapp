// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.Daneel.ViewControllers
{
	[Register ("AboutViewController")]
	partial class AboutViewController
	{
		[Outlet]
		UIKit.UIView LicensesView { get; set; }

        [Outlet]
        UIKit.UIView PrivacyPolicyView { get; set; }

        [Outlet]
        UIKit.UIView TermsOfServiceView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint TopConstraint { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (LicensesView != null) {
				LicensesView.Dispose ();
				LicensesView = null;
			}

            if (PrivacyPolicyView != null)
            {
                PrivacyPolicyView.Dispose();
                PrivacyPolicyView = null;
            }

            if (TermsOfServiceView != null)
            {
                TermsOfServiceView.Dispose();
                TermsOfServiceView = null;
            }

			if (TopConstraint != null) {
				TopConstraint.Dispose ();
				TopConstraint = null;
			}
		}
	}
}
