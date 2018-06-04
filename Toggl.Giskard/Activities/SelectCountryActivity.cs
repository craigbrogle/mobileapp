using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Views;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme",
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed class SelectCountryActivity : MvxAppCompatActivity<SelectCountryViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            this.ChangeStatusBarColor(new Color(ContextCompat.GetColor(this, Resource.Color.lightToolbarBackground)));

            base.OnCreate(bundle);
            SetContentView(Resource.Layout.SelectCountryActivity);

            OverridePendingTransition(Resource.Animation.abc_slide_in_bottom, Resource.Animation.abc_fade_out);
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_slide_out_bottom);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                ViewModel.CloseCommand.Execute();
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }
    }
}