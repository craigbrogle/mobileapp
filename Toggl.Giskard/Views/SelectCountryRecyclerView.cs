using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Giskard.Adapters;
using Android.Views;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Autocomplete.Suggestions;

namespace Toggl.Giskard.Views
{
    [Register("toggl.giskard.views.SelectCountryRecyclerView")]
    public sealed class SelectCountryRecyclerView : MvxRecyclerView
    {
        public SelectCountryRecyclerView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public SelectCountryRecyclerView(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {
        }

        public SelectCountryRecyclerView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle, new MvxRecyclerAdapter())
        {
            SetItemViewCacheSize(20);
            DrawingCacheEnabled = true;
            DrawingCacheQuality = DrawingCacheQuality.High;
        }
    }
}