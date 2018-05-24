using System;
using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Java.Lang;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V4;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using Toggl.Giskard.Views;
using Object = Java.Lang.Object;

namespace Toggl.Giskard.Adapters
{
    public sealed class SelectTimePagerAdapter : PagerAdapter
    {
        private int[] resources =
        {
            Resource.Layout.SelectStartDateTimeEditorPage,
            Resource.Layout.SelectStopDateTimeEditorPage,
            Resource.Layout.SelectTimeDurationPage
        };

        private readonly IMvxAndroidBindingContext bindingContext;

        public SelectTimePagerAdapter(IMvxBindingContext bindingContext)
        {
            this.bindingContext = (IMvxAndroidBindingContext)bindingContext;
        }

        public SelectTimePagerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override int Count => resources.Length;

        public override Object InstantiateItem(ViewGroup container, int position)
        {
            var resourceId = resources[position];

            var inflatedView = bindingContext.BindingInflate(resourceId, null);
            container.AddView(inflatedView);

            inflatedView.RequestLayout();

            return inflatedView;
        }

        public override void DestroyItem(ViewGroup container, int position, Object @object)
        {
            container.RemoveView(@object as View);
        }

        public override bool IsViewFromObject(View view, Object @object)
            => view == @object;
    }
}
