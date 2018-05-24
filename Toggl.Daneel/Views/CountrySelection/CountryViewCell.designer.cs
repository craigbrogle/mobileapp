// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.Daneel.Views.CountrySelection
{
	[Register ("CountryViewCell")]
	partial class CountryViewCell
	{
		[Outlet]
		UIKit.UIImageView CheckBoxImageView { get; set; }

		[Outlet]
		UIKit.UILabel NameLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (NameLabel != null) {
				NameLabel.Dispose ();
				NameLabel = null;
			}

			if (CheckBoxImageView != null) {
				CheckBoxImageView.Dispose ();
				CheckBoxImageView = null;
			}
		}
	}
}
