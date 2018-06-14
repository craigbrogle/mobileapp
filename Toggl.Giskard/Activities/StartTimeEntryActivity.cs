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
using System.Reactive.Disposables;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme",
              ScreenOrientation = ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed partial class StartTimeEntryActivity : MvxAppCompatActivity<StartTimeEntryViewModel>, IReactiveBindingHolder
    {
        public CompositeDisposable DisposeBag { get; private set; } = new CompositeDisposable();

        protected override void OnCreate(Bundle bundle)
        {
            this.ChangeStatusBarColor(new Color(ContextCompat.GetColor(this, Resource.Color.blueStatusBarBackground)));

            base.OnCreate(bundle);
            SetContentView(Resource.Layout.StartTimeEntryActivity);

            OverridePendingTransition(Resource.Animation.abc_slide_in_bottom, Resource.Animation.abc_fade_out);

            initializeViews();
            setupBindings();
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
            FindViewById<EditText>(Resource.Id.StartTimeEntryDescriptionTextField).RequestFocus();
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_slide_out_bottom);
        }
    }
}
