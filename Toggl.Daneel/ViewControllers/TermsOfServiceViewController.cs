using System;
using CoreGraphics;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.iOS.Views;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;
using static Toggl.Daneel.Extensions.RangeExtensions;

namespace Toggl.Daneel.ViewControllers
{
    [ModalDialogPresentation]
    public sealed partial class TermsOfServiceViewController
        : MvxViewController<TermsOfServiceViewModel>
    {
        private const int fontSize = 15;

        private readonly NSRange termsOfServiceTextRange = new NSRange(56, 16);
        private readonly NSRange privacyPolicyRange = new NSRange(77, 14);

        private readonly UIStringAttributes normalTextAttributes = new UIStringAttributes
        {
            Font = UIFont.SystemFontOfSize(fontSize)
        };
        private readonly UIStringAttributes highlitedTextAttributes = new UIStringAttributes
        {
            Font = UIFont.SystemFontOfSize(fontSize),
            ForegroundColor = Color.Signup.HighlightedText.ToNativeColor()
        };

        public TermsOfServiceViewController() : base(nameof(TermsOfServiceViewController), null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            PreferredContentSize = new CGSize(View.Frame.Width, View.Frame.Height);
            prepareTextView();

            var bindingSet = this.CreateBindingSet<TermsOfServiceViewController, TermsOfServiceViewModel>();

            bindingSet.Bind(CloseButton).To(vm => vm.CloseCommand);
            bindingSet.Bind(AcceptButton).To(vm => vm.AcceptCommand);

            bindingSet.Apply();
        }

        private void prepareTextView()
        {
            TextView.TextContainerInset = UIEdgeInsets.Zero;
            TextView.TextContainer.LineFragmentPadding = 0;

            var text = new NSMutableAttributedString(Resources.TermsOfServiceDialogMessage, normalTextAttributes);
            text.AddAttributes(highlitedTextAttributes, termsOfServiceTextRange);
            text.AddAttributes(highlitedTextAttributes, privacyPolicyRange);
            TextView.AttributedText = text;

            TextView.AddGestureRecognizer(new UITapGestureRecognizer(onTextViewTapped));
        }

        private void onTextViewTapped(UITapGestureRecognizer recognizer)
        {
            var layoutManager = TextView.LayoutManager;
            var location = recognizer.LocationInView(TextView);
            location.X -= TextView.TextContainerInset.Left;
            location.Y -= TextView.TextContainerInset.Top;

            nfloat _ = 0;
            var characterIndex = layoutManager.CharacterIndexForPoint(location, TextView.TextContainer, ref _);

            if (termsOfServiceTextRange.ContainsNumber(characterIndex))
                ViewModel.ViewTermsOfServiceCommand.Execute();

            if (privacyPolicyRange.ContainsNumber(characterIndex))
                ViewModel.ViewPrivacyPolicyCommand.Execute();
        }
    }
}
