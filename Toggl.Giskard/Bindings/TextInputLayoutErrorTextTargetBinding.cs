using Android.Support.Design.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;

namespace Toggl.Giskard.Bindings
{
    public class TextInputLayoutErrorTextTargetBinding : MvxTargetBinding<TextInputLayout, string>
    {
        public const string BindingName = "ErrorText";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        public TextInputLayoutErrorTextTargetBinding(TextInputLayout target) : base(target)
        {
        }

        protected override void SetValue(string value)
        {
            Target.Error = value;
        }
    }
}
