using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Giskard.Adapters;

namespace Toggl.Giskard.Views
{
    [Register("toggl.giskard.views.CalendarRecyclerView")]
    public sealed class CalendarRecyclerView : MvxRecyclerView
    {
        public CalendarRecyclerView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public CalendarRecyclerView(Context context)
            : this(context, null)
        {
        }

        public CalendarRecyclerView(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {
        }

        public CalendarRecyclerView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle, new CalendarRecyclerAdapter())
        {
            SetLayoutManager(new CalendarLayoutManager(context));
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var width = MeasureSpec.GetSize(widthMeasureSpec);
            var offset = width % 7;
            var newWidth = width - offset; 
            var newWidthMeasureSpec = MeasureSpec.MakeMeasureSpec(newWidth, MeasureSpecMode.Exactly);
            base.OnMeasure(newWidthMeasureSpec, heightMeasureSpec);
        }

        private class CalendarLayoutManager : GridLayoutManager
        {
            public CalendarLayoutManager(Context context)
                : base(context, 7, LinearLayoutManager.Vertical, false)
            {
            }

            public override bool CanScrollVertically() => false;
        }
    }
}
