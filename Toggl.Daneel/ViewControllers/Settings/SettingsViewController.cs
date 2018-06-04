using System;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.iOS.Views;
using MvvmCross.iOS.Views.Presenters.Attributes;
using MvvmCross.Plugins.Color.iOS;
using MvvmCross.Plugins.Visibility;
using Toggl.Daneel.Extensions;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewControllers
{
    [MvxChildPresentation]
    public partial class SettingsViewController : MvxViewController<SettingsViewModel>
    {
        private IDisposable willEnterForegroundNotification;

        private const int verticalSpacing = 24;

        public SettingsViewController() 
            : base(nameof(SettingsViewController), null)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing == false) return;

            willEnterForegroundNotification?.Dispose();
            willEnterForegroundNotification = null;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            prepareViews();

            Title = ViewModel.Title;

            var inverseBoolConverter = new BoolToConstantValueConverter<bool>(false, true);
            var visibilityConverter = new MvxVisibilityValueConverter();
            var durationFormatToStringConverter = new DurationFormatToStringValueConverter();

            var bindingSet = this.CreateBindingSet<SettingsViewController, SettingsViewModel>();

            // Text
            bindingSet.Bind(EmailLabel).To(vm => vm.Email);
            bindingSet.Bind(WorkspaceLabel).To(vm => vm.WorkspaceName);
            bindingSet.Bind(DateFormatLabel).To(vm => vm.DateFormat.Localized);
            bindingSet.Bind(DurationFormatLabel)
                      .To(vm => vm.DurationFormat)
                      .WithConversion(durationFormatToStringConverter);
            bindingSet.Bind(BeginningOfWeekLabel).To(vm => vm.BeginningOfWeek);
            bindingSet.Bind(VersionLabel).To(vm => vm.Version);

            // Commands
            bindingSet.Bind(LogoutButton).To(vm => vm.LogoutCommand);
            bindingSet.Bind(EmailView)
                      .For(v => v.BindTap())
                      .To(vm => vm.EditProfileCommand);

            bindingSet.Bind(DateFormatView)
                      .For(v => v.BindTap())
                      .To(vm => vm.SelectDateFormatCommand);

            bindingSet.Bind(DurationFormatView)
                      .For(verticalSpacing => verticalSpacing.BindTap())
                      .To(vm => vm.SelectDurationFormatCommand);

            bindingSet.Bind(WorkspaceView)
                      .For(v => v.BindTap())
                      .To(vm => vm.PickWorkspaceCommand);

            bindingSet.Bind(ManualModeView)
                      .For(v => v.BindTap())
                      .To(vm => vm.ToggleManualModeCommand);

            bindingSet.Bind(TwentyFourHourClockView)
                      .For(v => v.BindTap())
                      .To(vm => vm.ToggleUseTwentyFourHourClockCommand);

            bindingSet.Bind(TwentyFourHourClockSwitch)
                      .For(v => v.BindValueChanged())
                      .To(vm => vm.ToggleUseTwentyFourHourClockCommand);

            bindingSet.Bind(BeginningOfWeekView)
                      .For(v => v.BindTap())
                      .To(vm => vm.SelectBeginningOfWeekCommand);

            bindingSet.Bind(FeedbackView)
                      .For(v => v.BindTap())
                      .To(vm => vm.SubmitFeedbackCommand);

            bindingSet.Bind(AboutView)
                      .For(v => v.BindTap())
                      .To(vm => vm.AboutCommand);

            bindingSet.Bind(HelpView)
                      .For(v => v.BindTap())
                      .To(vm => vm.HelpCommand);

            // Logout process
            bindingSet.Bind(LogoutButton)
                      .For(btn => btn.Enabled)
                      .To(vm => vm.IsLoggingOut)
                      .WithConversion(inverseBoolConverter);

            bindingSet.Bind(NavigationItem)
                      .For(nav => nav.BindHidesBackButton())
                      .To(vm => vm.IsLoggingOut);

            bindingSet.Bind(SyncingView)
                      .For(view => view.BindVisibility())
                      .To(vm => vm.IsRunningSync)
                      .WithConversion(visibilityConverter);

            bindingSet.Bind(SyncedView)
                      .For(view => view.BindVisibility())
                      .To(vm => vm.IsSynced)
                      .WithConversion(visibilityConverter);

            bindingSet.Bind(LoggingOutView)
                      .For(view => view.BindVisibility())
                      .To(vm => vm.IsLoggingOut)
                      .WithConversion(visibilityConverter);

            // Switches
            bindingSet.Bind(TwentyFourHourClockSwitch)
                      .For(v => v.BindAnimatedOn())
                      .To(vm => vm.UseTwentyFourHourClock);

            bindingSet.Bind(ManualModeSwitch)
                      .For(v => v.BindAnimatedOn())
                      .To(vm => vm.IsManualModeEnabled);

            bindingSet.Apply();

            willEnterForegroundNotification = UIApplication.Notifications.ObserveWillEnterForeground((sender, e) => startAnimations());
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            startAnimations();
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
            tryAlignLogoutButtonWithBottomEdge();
        }

        private void prepareViews()
        {
            // Syncing indicator colors
            setIndicatorSyncColor(SyncedIcon);
            setIndicatorSyncColor(SyncingIndicator);
            setIndicatorSyncColor(LoggingOutIndicator);

            // Resize Switches
            TwentyFourHourClockSwitch.Resize();
            ManualModeSwitch.Resize();
        }

        private void setIndicatorSyncColor(UIImageView imageView)
        {
            imageView.Image = imageView.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            imageView.TintColor = Color.Settings.SyncStatusText.ToNativeColor();
        }

        private void startAnimations()
        {
            SyncingActivityIndicatorView.StartAnimation();
            LoggingOutActivityIndicatorView.StartAnimation();
        }

        private void tryAlignLogoutButtonWithBottomEdge()
        {
            var contentHeight = LogoutContainerView.Frame.Top - LogoutVerticalOffsetConstraint.Constant + LogoutContainerView.Frame.Height;
            var bottomOffset = verticalSpacing;
            var idealDistance = ScrollView.Frame.Height - contentHeight - bottomOffset;
            var distance = Math.Max(idealDistance, verticalSpacing);
            LogoutVerticalOffsetConstraint.Constant = (nfloat)distance;
        }
    }
}
