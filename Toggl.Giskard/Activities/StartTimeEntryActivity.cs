using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Content;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using MvvmCross.Droid.Platform;
using MvvmCross.Platform.Platform;
using MvvmCross.Platform;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme",
              ScreenOrientation = ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed class StartTimeEntryActivity : MvxAppCompatActivity<StartTimeEntryViewModel>
    {
        private EditText descriptionTextField { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            this.ChangeStatusBarColor(new Color(ContextCompat.GetColor(this, Resource.Color.blueStatusBarBackground)));

            base.OnCreate(bundle);
            SetContentView(Resource.Layout.StartTimeEntryActivity);
            descriptionTextField = FindViewById<EditText>(Resource.Id.StartTimeEntryDescriptionTextField);

            OverridePendingTransition(Resource.Animation.abc_slide_in_bottom, Resource.Animation.abc_fade_out);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                ViewModel.BackCommand.ExecuteAsync();
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        protected override void OnResume()
        {
            base.OnResume();
            descriptionTextField.RequestFocus();
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_slide_out_bottom);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
            descriptionTextField.Invalidate();
        }
    }
}
