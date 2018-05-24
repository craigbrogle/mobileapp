using Android.Views;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Target;

namespace Toggl.Giskard.Bindings
{
    public sealed class ViewUpsideDownTargetBinding : MvxAndroidTargetBinding<View, bool>
    {
        public const string BindingName = "UpsideDown";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        public ViewUpsideDownTargetBinding(View target)
                : base(target)
        {
        }

        protected override void SetValueImpl(View target, bool value)
        {
            var angle = value ? 180.0f : 0.0f;
            target.Animate().SetDuration(1).Rotation(angle);
        }
    }
}
