using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme",
              ScreenOrientation = ScreenOrientation.Portrait,
              WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed class SignUpActivity : MvxAppCompatActivity<SignupViewModel>
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            this.ChangeStatusBarColor(Color.White, true);

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SignUpActivity);
        }
    }
}
