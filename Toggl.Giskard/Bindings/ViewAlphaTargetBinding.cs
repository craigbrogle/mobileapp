using System;
using Android.Graphics.Drawables;
using Android.Views;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Target;
using Toggl.Giskard.Extensions;
using MvvmCross.Plugins.Color.Droid;
using MvvmCross.Platform.UI;
using Android.Graphics;
using Android.Widget;

namespace Toggl.Giskard.Bindings
{
    public sealed class ViewAlphaTargetBinding : MvxAndroidTargetBinding<View, float>
    {
        public const string BindingName = "Alpha";

        public ViewAlphaTargetBinding(View target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValueImpl(View target, float value)
        {
            target.Alpha = value;
        }
    }
}
