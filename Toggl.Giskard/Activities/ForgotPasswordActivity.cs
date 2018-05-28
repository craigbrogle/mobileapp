using System;
using System.ComponentModel;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views.Attributes;
using MvvmCross.Platform.WeakSubscription;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.StateVisible,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed class ForgotPasswordActivity : MvxAppCompatActivity<ForgotPasswordViewModel>
    {
        private IDisposable passwordResetSuccessfullyDisposable;
        private IDisposable errorDisposable;
        private EditText loginEmailEditText;

        protected override void OnCreate(Bundle bundle)
        {
            this.ChangeStatusBarColor(Color.White, true);

            base.OnCreate(bundle);
            SetContentView(Resource.Layout.ForgotPasswordActivity);

            setupToolbar();

            loginEmailEditText = FindViewById<EditText>(Resource.Id.LoginEmailEditText);
            setupInputField();

            passwordResetSuccessfullyDisposable =
                ViewModel.WeakSubscribe(() => ViewModel.PasswordResetSuccessful, showResetPasswordSuccessToast);
        }

        private void setupInputField()
        {
            loginEmailEditText.SetFocus();
            loginEmailEditText.SetSelection(loginEmailEditText.Text?.Length ?? 0);
        }

        private void showResetPasswordSuccessToast(object sender, PropertyChangedEventArgs e)
        {
            loginEmailEditText.RemoveFocus();
            Toast.MakeText(this, Resource.String.ResetPasswordEmailSentMessage, ToastLength.Long).Show();
        }

        private void setupToolbar()
        {
            var toolbar = FindViewById<Toolbar>(Resource.Id.ForgotPasswordToolbar);

            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.Title = GetString(Resource.String.ForgotPasswordTitle);

            toolbar.NavigationClick += navigationClick;
        }

        private void navigationClick(object sender, Toolbar.NavigationClickEventArgs args)
        {
            executeBackCommand();
        }

        private void executeBackCommand()
        {
            if (ViewModel.IsLoading) return;

            loginEmailEditText.RemoveFocus();
            ViewModel.CloseCommand.Execute();
        }
    }
}
