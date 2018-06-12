using System;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using Toggl.Giskard.Helper;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using static Toggl.Multivac.Extensions.CommonFunctions;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme",
              ScreenOrientation = ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed class SettingsActivity : MvxAppCompatActivity<SettingsViewModel>
    {
        private LinearLayout avatarContainer;
        private IDisposable avatarBitmapDisposable;
        private ImageView avatarView;

        protected override void OnCreate(Bundle bundle)
        {
            this.ChangeStatusBarColor(Color.ParseColor("#2C2C2C"));

            base.OnCreate(bundle);
            SetContentView(Resource.Layout.SettingsActivity);

            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);

            avatarView = FindViewById<ImageView>(Resource.Id.SettingsViewAvatarImage);
            avatarContainer = FindViewById<LinearLayout>(Resource.Id.SettingsViewAvatarImageContainer);

            setupToolbar();
            setupAvatar();
        }

        private void setupAvatar()
        {
            if (string.IsNullOrEmpty(ViewModel.ImageUrl))
                return;

            avatarBitmapDisposable = ImageUtils.GetImageFromUrl(ViewModel.ImageUrl)
                                               .ObserveOn(SynchronizationContext.Current)
                                               .Subscribe(bitmap =>
                                               {
                                                   avatarView.SetImageBitmap(bitmap);
                                                   avatarContainer.Visibility = ViewStates.Visible;
                                               }, onError: DoNothing);
        }

        private void setupToolbar()
        {
            var toolbar = FindViewById<Toolbar>(Resource.Id.Toolbar);

            toolbar.Title = ViewModel.Title;

            SetSupportActionBar(toolbar);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);

            toolbar.NavigationClick += onNavigateBack;
        }

        private void onNavigateBack(object sender, Toolbar.NavigationClickEventArgs e)
        {
            ViewModel.BackCommand.Execute();
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing == false) return;

            avatarBitmapDisposable?.Dispose();
        }
    }
}
