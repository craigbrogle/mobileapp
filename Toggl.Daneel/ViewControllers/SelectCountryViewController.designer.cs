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
	[Register ("SelectCountryViewController")]
	partial class SelectCountryViewController
	{
		[Outlet]
		UIKit.NSLayoutConstraint BottomConstraint { get; set; }

		[Outlet]
		UIKit.UIButton CloseButton { get; set; }

		[Outlet]
		UIKit.UITableView CountriesTableView { get; set; }

		[Outlet]
		UIKit.UITextField SearchTextField { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (BottomConstraint != null) {
				BottomConstraint.Dispose ();
				BottomConstraint = null;
			}

			if (CloseButton != null) {
				CloseButton.Dispose ();
				CloseButton = null;
			}

			if (SearchTextField != null) {
				SearchTextField.Dispose ();
				SearchTextField = null;
			}

			if (CountriesTableView != null) {
				CountriesTableView.Dispose ();
				CountriesTableView = null;
			}
		}
	}
}
