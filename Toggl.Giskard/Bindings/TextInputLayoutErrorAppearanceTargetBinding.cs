using Android.Support.Design.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;

namespace Toggl.Giskard.Bindings
{
    public class TextInputLayoutErrorAppearanceTargetBinding : MvxTargetBinding<TextInputLayout, bool>
    {
        public const string BindingName = "ErrorTextAppearance";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        public TextInputLayoutErrorAppearanceTargetBinding(TextInputLayout target) : base(target)
        {
        }

        protected override void SetValue(bool value)
        {
            Target.SetErrorTextAppearance(value
                ? Resource.Style.TextInputLayoutErrorAppearance
                : Resource.Style.TextInputLayoutRegularTextAppearance
            );
        }
    }
}
