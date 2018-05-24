using System;
using System.ComponentModel;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Droid.Views.Attributes;
using MvvmCross.Platform;
using MvvmCross.Platform.WeakSubscription;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Activities;
using Toggl.Giskard.Adapters;
using Toggl.Giskard.Extensions;
using Toggl.Multivac.Extensions;

namespace Toggl.Giskard.Fragments
{
    [MvxFragmentPresentation(typeof(ReportsViewModel), Resource.Id.ReportsCalendarContainer, AddToBackStack = false)]
    public sealed class ReportsCalendarFragment : MvxFragment<ReportsCalendarViewModel>
    {
        private int rowHeight;
        private ViewPager pager;
        private int currentRowCount;
        private IDisposable disposable;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = this.BindingInflate(Resource.Layout.ReportsCalendarFragment, null);

            rowHeight = Activity.Resources.DisplayMetrics.WidthPixels / 7;

            Mvx.Resolve<IMvxBindingContextStack<IMvxAndroidBindingContext>>()
               .Push(BindingContext as IMvxAndroidBindingContext);

            pager = view.FindViewById<ViewPager>(Resource.Id.ReportsCalendarFragmentViewPager);
            pager.Adapter = new CalendarPagerAdapter(Activity, ViewModel);
            pager.SetCurrentItem(ViewModel.Months.Count - 1, false);

            view.FindViewById<MvxRecyclerView>(Resource.Id.ReportsCalendarFragmentShortcuts)
                .SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));

            view.FindViewById<LinearLayout>(Resource.Id.ReportsCalendarFragmentHeader)
                .GetChildren<TextView>()
                .Indexed()
                .ForEach((textView, index)
                    => textView.Text = ViewModel.DayHeaderFor(index));

            disposable =
                ViewModel.WeakSubscribe<PropertyChangedEventArgs>(nameof(ViewModel.RowsInCurrentMonth), onRowCountChanged);

            recalculatePagerHeight();

            return view;
        }

        private void onRowCountChanged(object sender, PropertyChangedEventArgs e)
        {
            if (currentRowCount == ViewModel.RowsInCurrentMonth)
                return;

            recalculatePagerHeight();
        }

        private void recalculatePagerHeight()
        {
            currentRowCount = ViewModel.RowsInCurrentMonth;

            var layoutParams = pager.LayoutParameters;
            layoutParams.Height = rowHeight * currentRowCount;
            pager.LayoutParameters = layoutParams;

            var activity = (ReportsActivity)Activity;
            activity.RecalculateCalendarHeight();
        }
    }
}
