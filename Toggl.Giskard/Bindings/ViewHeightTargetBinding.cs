using System;
using Android.Views;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Target;
using Toggl.Giskard.Extensions;

namespace Toggl.Giskard.Bindings
{
    public sealed class ViewHeightTargetBinding : MvxAndroidTargetBinding<View, int>
    {
        public const string BindingName = "Height";

        public ViewHeightTargetBinding(View target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValueImpl(View target, int value)
        {
            target.Post(() =>
            {
                var layoutParams = target.LayoutParameters;
                layoutParams.Height = value.DpToPixels(target.Context);
                target.LayoutParameters = layoutParams;
            });
        }
    }
}
