using System;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Daneel.Views;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.Sync;
using Toggl.Multivac;
using UIKit;
using static Toggl.Daneel.Extensions.TextExtensions;
using static Toggl.Foundation.Sync.SyncProgress;

namespace Toggl.Daneel.ViewSources
{
    public sealed class SwipeToRefreshTableViewDelegate : UITableViewDelegate
    {
        private const float syncBarHeight = 26;
        private const float activityIndicatorSize = 14;
        private const float syncLabelFontSize = 12;

        private static readonly float scrollThreshold = 3 * syncBarHeight;

        private readonly UITableView tableView;
        private readonly UIColor pullToRefreshColor = Color.Main.PullToRefresh.ToNativeColor();
        private readonly UIColor syncingColor = Color.Main.Syncing.ToNativeColor();
        private readonly UIColor syncFailedColor = Color.Main.SyncFailed.ToNativeColor();
        private readonly UIColor offlineColor = Color.Main.Offline.ToNativeColor();
        private readonly UIColor syncCompletedColor = Color.Main.SyncCompleted.ToNativeColor();

        private SyncProgress syncProgress;
        private bool wasReleased;
        private bool needsRefresh;
        private bool shouldCalculateOnDeceleration;
        private int syncIndicatorLastShown;
        private bool shouldRefreshOnTap;
        private bool showBarAutomatically;

        public SyncProgress SyncProgress
        {
            get => syncProgress;
            set
            {
                if (value == syncProgress) return;
                syncProgress = value;
                OnSyncProgressChanged();
            }
        }

        private readonly UIView syncStateView = new UIView();
        private readonly UILabel syncStateLabel = new UILabel();
        private readonly UIButton dismissSyncBarButton = new UIButton();
        private readonly ActivityIndicatorView activityIndicatorView = new ActivityIndicatorView();

        public IMvxCommand RefreshCommand { get; set; }

        public SwipeToRefreshTableViewDelegate(UITableView tableView)
        {
            Ensure.Argument.IsNotNull(tableView, nameof(tableView));

            this.tableView = tableView;
        }

        public void Initialize()
        {
            syncStateView.AddSubview(syncStateLabel);
            syncStateView.AddSubview(activityIndicatorView);
            syncStateView.AddSubview(dismissSyncBarButton);
            tableView.AddSubview(syncStateView);

            prepareSyncStateView();
            prepareSyncStateLabel();
            prepareActivityIndicatorView();
            prepareDismissSyncBarImageView();
        }

        private void prepareSyncStateView()
        {
            syncStateView.BackgroundColor = pullToRefreshColor;
            syncStateView.TranslatesAutoresizingMaskIntoConstraints = false;
            syncStateView.BottomAnchor.ConstraintEqualTo(tableView.TopAnchor).Active = true;
            syncStateView.WidthAnchor.ConstraintEqualTo(tableView.WidthAnchor).Active = true;
            syncStateView.CenterXAnchor.ConstraintEqualTo(tableView.CenterXAnchor).Active = true;
            syncStateView.TopAnchor.ConstraintEqualTo(tableView.Superview.TopAnchor).Active = true;
        }

        private void prepareSyncStateLabel()
        {
            syncStateLabel.TextColor = UIColor.White;
            syncStateLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            syncStateLabel.Font = syncStateLabel.Font.WithSize(syncLabelFontSize);
            syncStateLabel.CenterXAnchor.ConstraintEqualTo(syncStateView.CenterXAnchor).Active = true;
            syncStateLabel.BottomAnchor.ConstraintEqualTo(syncStateView.BottomAnchor, -6).Active = true;

            var tapGestureRecognizer = new UITapGestureRecognizer(onStatusLabelTap);
            syncStateLabel.UserInteractionEnabled = true;
            syncStateLabel.AddGestureRecognizer(tapGestureRecognizer);
        }

        private void prepareActivityIndicatorView()
        {
            activityIndicatorView.TranslatesAutoresizingMaskIntoConstraints = false;
            activityIndicatorView.WidthAnchor.ConstraintEqualTo(activityIndicatorSize).Active = true;
            activityIndicatorView.HeightAnchor.ConstraintEqualTo(activityIndicatorSize).Active = true;
            activityIndicatorView.CenterYAnchor.ConstraintEqualTo(syncStateLabel.CenterYAnchor).Active = true;
            activityIndicatorView.LeadingAnchor.ConstraintEqualTo(syncStateLabel.TrailingAnchor, 6).Active = true;
            activityIndicatorView.StartAnimation();
        }

        private void prepareDismissSyncBarImageView()
        {
            dismissSyncBarButton.Hidden = true;
            dismissSyncBarButton.SetImage(UIImage.FromBundle("icClose").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            dismissSyncBarButton.TintColor = UIColor.White;
            dismissSyncBarButton.TranslatesAutoresizingMaskIntoConstraints = false;
            dismissSyncBarButton.CenterYAnchor.ConstraintEqualTo(syncStateLabel.CenterYAnchor).Active = true;
            dismissSyncBarButton.TrailingAnchor.ConstraintEqualTo(syncStateView.TrailingAnchor, -16).Active = true;
            dismissSyncBarButton.TouchUpInside += onDismissSyncBarButtonTap;
        }

        private async void OnSyncProgressChanged()
        {
            if (SyncProgress == Unknown)
                return;

            var showBar = showBarAutomatically || SyncProgress == OfflineModeDetected || SyncProgress == Failed;
            var hideBarAutomatically = SyncProgress == Synced;
            var showSpinner = SyncProgress == Syncing;
            var showDismissButton = SyncProgress == OfflineModeDetected || SyncProgress == Failed;
            shouldRefreshOnTap = SyncProgress == OfflineModeDetected;
            showBarAutomatically = SyncProgress != Synced && showBarAutomatically;

            var (text, backgroundColor) = getSyncIndicatorTextAndBackgroundBasedOnCurrentProgress();
            setSyncIndicatorTextAndBackground(text, backgroundColor);
            setActivityIndicatorVisible(showSpinner);
            dismissSyncBarButton.Hidden = !showDismissButton;

            if (!showBar) return;

            int syncIndicatorShown = showSyncBar();

            if (!hideBarAutomatically) return;

            await hideSyncBar(syncIndicatorShown);
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            if (!scrollView.Dragging || wasReleased) return;

            var offset = scrollView.ContentOffset.Y;
            if (offset >= 0) return;

            var needsMorePulling = System.Math.Abs(offset) < scrollThreshold;
            needsRefresh = !needsMorePulling;

            if (SyncProgress == Syncing) return;

            dismissSyncBarButton.Hidden = true;
            setSyncIndicatorTextAndBackground(
                new NSAttributedString(needsMorePulling ? Resources.PullDownToRefresh : Resources.ReleaseToRefresh),
                pullToRefreshColor);
        }

        public override void DraggingStarted(UIScrollView scrollView)
        {
            wasReleased = false;
        }

        public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
        {
            var offset = scrollView.ContentOffset.Y;
            if (offset >= 0) return;

            shouldCalculateOnDeceleration = willDecelerate;
            wasReleased = true;

            if (shouldCalculateOnDeceleration) return;
            refreshIfNeeded();
        }

        public override void DecelerationEnded(UIScrollView scrollView)
        {
            if (!shouldCalculateOnDeceleration) return;
            refreshIfNeeded();
        }

        private void refreshIfNeeded()
        {
            if (!needsRefresh) return;

            needsRefresh = false;
            shouldCalculateOnDeceleration = false;
            showBarAutomatically = true;

            if (SyncProgress == Syncing)
            {
                showSyncBar();
                return;
            }

            RefreshCommand?.Execute();
        }

        private (NSAttributedString, UIColor) getSyncIndicatorTextAndBackgroundBasedOnCurrentProgress()
        {
            switch (SyncProgress)
            {
                case Syncing:
                    return (new NSAttributedString(Resources.Syncing), syncingColor);

                case OfflineModeDetected:
                    return (Resources.Offline.EndingWithRefreshIcon(syncStateLabel.Font.CapHeight), offlineColor);

                case Synced:
                    return (Resources.SyncCompleted.EndingWithTick(syncStateLabel.Font.CapHeight), syncCompletedColor);

                case Failed:
                    return (Resources.SyncFailed.EndingWithRefreshIcon(syncStateLabel.Font.CapHeight), syncFailedColor);

                default:
                    throw new ArgumentException(nameof(SyncProgress));
            }
        }

        private void setSyncIndicatorTextAndBackground(NSAttributedString text, UIColor backgroundColor)
        {
            UIView.Animate(Animation.Timings.EnterTiming, () =>
            {
                syncStateLabel.AttributedText = text;
                syncStateView.BackgroundColor = backgroundColor;
            });
        }

        private int showSyncBar()
        {
            if (tableView.Dragging) return syncIndicatorLastShown;
            if (tableView.ContentOffset.Y > 0) return syncIndicatorLastShown;

            tableView.SetContentOffset(new CGPoint(0, -syncBarHeight), true);
            return ++syncIndicatorLastShown;
        }

        private async Task hideSyncBar(int syncIndicatorShown)
        {
            await Task.Delay(Animation.Timings.HideSyncStateViewDelay);

            if (syncIndicatorShown != syncIndicatorLastShown) return;
            if (tableView.ContentOffset.Y > 0) return;

            tableView.SetContentOffset(CGPoint.Empty, true);
        }

        private void setActivityIndicatorVisible(bool visible)
        {
            activityIndicatorView.Hidden = !visible;

            if (visible)
                activityIndicatorView.StartAnimation();
            else
                activityIndicatorView.StopAnimation();
        }

        private void onStatusLabelTap()
        {
            if (shouldRefreshOnTap == false) return;

            RefreshCommand?.Execute();
        }

        private void onDismissSyncBarButtonTap(object sender, EventArgs e)
        {
            tableView.SetContentOffset(CGPoint.Empty, true);
        }
    }
}
