using System;
using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Views.Attributes;
using MvvmCross.Platform.WeakSubscription;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Adapters;
using Toggl.Giskard.Extensions;
using Toggl.Multivac.Extensions;

namespace Toggl.Giskard.Fragments
{
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    using MvvmCross.Binding.BindingContext;
    using Toggl.Giskard.Helper;
    using static SelectTimeFragment.EditorMode;
    using static SelectTimeViewModel;
    using static SelectTimeViewModel.TemporalInconsistency;

    [MvxDialogFragmentPresentation(AddToBackStack = true)]
    public sealed class SelectTimeFragment : MvxDialogFragment<SelectTimeViewModel>, TabLayout.IOnTabSelectedListener
    {
        internal enum EditorMode
        {
            Date,
            Time,
            Duration,
            RunningTimeEntry
        }

        private readonly int[] heights = { 450, 400, 224, 204 };
        private const int vibrationDuration = 250;

        private EditorMode editorMode = Date;

        private IDisposable onModeChangedDisposable;
        private IDisposable onTemporalInconsistencyDisposable;

        private LinearLayout controlButtons;
        private TabLayout tabLayout;
        private ViewPager pager;

        private Toast toast;
        private Vibrator vibrator;

        public SelectTimeFragment()
        {
        }

        public SelectTimeFragment(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            vibrator = (Vibrator)Context.GetSystemService(Context.VibratorService);

            base.OnCreateView(inflater, container, savedInstanceState);
            var view = this.BindingInflate(Resource.Layout.SelectTimeFragment, null);

            controlButtons = view.FindViewById<LinearLayout>(Resource.Id.SelectTimeFragmentControlButtons);
            pager = view.FindViewById<ViewPager>(Resource.Id.SelectTimeFragmentPager);
            tabLayout = view.FindViewById<TabLayout>(Resource.Id.SelectTimeTabView);

            pager.OffscreenPageLimit = 2;
            pager.Adapter = new SelectTimePagerAdapter(BindingContext);
            tabLayout.AddOnTabSelectedListener(this);

            onModeChangedDisposable =
                ViewModel.WeakSubscribe<PropertyChangedEventArgs>(nameof(ViewModel.IsCalendarView), onIsCalendarViewChanged);

            onTemporalInconsistencyDisposable = ViewModel.TemporalInconsistencyDetected
                                                         .Subscribe(onTemporalInconsistency);

            var startPageView = this.BindingInflate(Resource.Layout.SelectDateTimeStartTimeTabHeader, null);
            var stopPageView = this.BindingInflate(Resource.Layout.SelectDateTimeStopTimeTabHeader, null);
            var durationPageView = this.BindingInflate(Resource.Layout.SelectDateTimeDurationTabHeader, null);

            tabLayout.Post(() =>
            {
                tabLayout.SetupWithViewPager(pager, true);

                tabLayout.GetTabAt(StartTimeTab).SetCustomView(startPageView);
                tabLayout.GetTabAt(StopTimeTab).SetCustomView(stopPageView);
                tabLayout.GetTabAt(DurationTab).SetCustomView(durationPageView);

                pager.SetCurrentItem(ViewModel.StartingTabIndex, false);
            });

            return view;
        }

        private Dictionary<TemporalInconsistency, int> inconsistencyMessages = new Dictionary<TemporalInconsistency, int>
        {
            [StartTimeAfterStopTime] = Resource.String.StartTimeAfterStopTimeWarning,
            [StopTimeBeforeStartTime] = Resource.String.StopTimeBeforeStartTimeWarning,
            [DurationTooLong] = Resource.String.DurationTooLong
        };

        private void onTemporalInconsistency(TemporalInconsistency temporalInconsistency)
        {
            if (toast != null)
                toast.Cancel();

            var messageResourceId = inconsistencyMessages[temporalInconsistency];
            var message = Resources.GetString(messageResourceId);

            toast = Toast.MakeText(Context, message, ToastLength.Short);
            toast.Show();
        }

        private void onIsCalendarViewChanged(object sender, PropertyChangedEventArgs args)
        {
            editorMode = ViewModel.IsCalendarView ? Date : Time;
            updateLayoutHeight();
        }

        public void OnTabReselected(TabLayout.Tab tab)
            => onTabChange(tab.Position);

        public void OnTabSelected(TabLayout.Tab tab)
            => onTabChange(tab.Position);

        private void onTabChange(int tabPosition)
        {
            ViewModel.CurrentTab = tabPosition;
            editorMode = calculateEditorMode();

            updateLayoutHeight();
        }

        private EditorMode calculateEditorMode()
        {
            if (ViewModel.CurrentTab == DurationTab)
                return Duration;

            if (ViewModel.CurrentTab == StopTimeTab && !ViewModel.IsTimeEntryStopped)
                return RunningTimeEntry;

            return ViewModel.IsCalendarView ? Date : Time;
        }

        public override void OnCancel(IDialogInterface dialog)
        {
            ViewModel.CancelCommand.Execute();
        }

        public override void OnDismiss(IDialogInterface dialog)
        {
            if (ViewModel == null) return;

            ViewModel.SaveCommand.Execute();
        }

        private void updateLayoutHeight()
        {
            var newHeight = heights[(int)editorMode];

            tabLayout.RequestLayout();
            controlButtons.ForceLayout();

            Activity.RunOnUiThread(() =>
            {
                var heightInPixels = newHeight.DpToPixels(Context);

                var pagerLayout = pager.LayoutParameters;
                pagerLayout.Height = heightInPixels;
                pager.LayoutParameters = pagerLayout;

                Dialog.Window.SetDefaultDialogLayout(Activity, Context, heightDp: ViewGroup.LayoutParams.WrapContent);
            });
        }

        public void OnTabUnselected(TabLayout.Tab tab)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing == false) return;

            onModeChangedDisposable.Dispose();
            onTemporalInconsistencyDisposable?.Dispose();
        }
    }
}
