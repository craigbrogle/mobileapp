using MvvmCross.Binding;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.iOS.Views;
using MvvmCross.iOS.Views.Presenters.Attributes;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Daneel.Extensions;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;
using static Toggl.Daneel.Extensions.LoginSignupViewExtensions;

namespace Toggl.Daneel.ViewControllers
{
    [MvxRootPresentation(WrapInNavigationController = true)]
    public sealed partial class SignupViewController : MvxViewController<SignupViewModel>
    {
        private const int iPhoneSeScreenHeight = 568;
        private const int topConstraintForBiggerScreens = 70;

        public SignupViewController() : base(nameof(SignupViewController), null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var signupButtonTitleConverter = new BoolToConstantValueConverter<string>("", Resources.SignUpTitle);

            var bindingSet = this.CreateBindingSet<SignupViewController, SignupViewModel>();

            //Text
            bindingSet.Bind(ErrorLabel).To(vm => vm.ErrorText);
            bindingSet.Bind(EmailTextField)
                      .To(vm => vm.Email)
                      .WithConversion(new EmailToStringValueConverter());

            bindingSet.Bind(PasswordTextField)
                      .To(vm => vm.Password)
                      .WithConversion(new PasswordToStringValueConverter());

            bindingSet.Bind(SignupButton)
                      .For(v => v.BindAnimatedTitle())
                      .To(vm => vm.IsLoading)
                      .WithConversion(signupButtonTitleConverter);

            bindingSet.Bind(SelectCountryButton)
                      .For(v => v.BindTitle())
                      .To(vm => vm.Country.Name);

            //Commands
            bindingSet.Bind(SignupButton).To(vm => vm.SignupCommand);
            bindingSet.Bind(GoogleSignupButton).To(vm => vm.GoogleSignupCommand);
            bindingSet.Bind(ShowPasswordButton).To(vm => vm.TogglePasswordVisibilityCommand);
            bindingSet.Bind(LoginCard)
                      .For(v => v.BindTap())
                      .To(vm => vm.LoginCommand);

            //Visibility
            bindingSet.Bind(ErrorLabel)
                      .For(v => v.BindAnimatedVisibility())
                      .To(vm => vm.HasError);

            bindingSet.Bind(ActivityIndicator)
                      .For(v => v.BindVisibilityWithFade())
                      .To(vm => vm.IsLoading);

            bindingSet.Bind(PasswordTextField)
                      .For(v => v.BindSecureTextEntry())
                      .To(vm => vm.IsPasswordMasked);

            bindingSet.Bind(ShowPasswordButton)
                      .For(v => v.BindVisible())
                      .To(vm => vm.IsShowPasswordButtonVisible);

            bindingSet.Bind(PasswordTextField)
                      .For(v => v.BindFirstResponder())
                      .To(vm => vm.IsShowPasswordButtonVisible)
                      .Mode(MvxBindingMode.OneWayToSource);
            
            bindingSet.Apply();

            prepareViews();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            if (View.Frame.Height > iPhoneSeScreenHeight)
                TopConstraint.Constant = topConstraintForBiggerScreens;

            LoginCard.SetupBottomCard();
            GoogleSignupButton.SetupGoogleButton();
        }

        private void prepareViews()
        {
            ActivityIndicator.Alpha = 0;
            ActivityIndicator.StartAnimation();

            SignupButton.SetTitleColor(
                Color.Login.DisabledButtonColor.ToNativeColor(),
                UIControlState.Disabled
            );

            EmailTextField.ShouldReturn += _ => {
                PasswordTextField.BecomeFirstResponder();
                return false;
            };

            PasswordTextField.ShouldReturn += _ =>
            {
                PasswordTextField.ResignFirstResponder();
                return false;
            };

            PasswordTextField.ResignFirstResponder();

            ShowPasswordButton.SetupShowPasswordButton();

            SelectCountryButton.SemanticContentAttribute = UISemanticContentAttribute.ForceRightToLeft;
            var spacing = 4;
            SelectCountryButton.ImageEdgeInsets = new UIEdgeInsets(0, spacing, 0, 0);
            SelectCountryButton.TitleEdgeInsets = new UIEdgeInsets(0, 0, 0, spacing);
        }
    }
}

