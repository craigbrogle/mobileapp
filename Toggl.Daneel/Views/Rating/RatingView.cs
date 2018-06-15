using System;
using CoreGraphics;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.Binding.iOS.Views;
using MvvmCross.Core.ViewModels;
using ObjCRuntime;
using Toggl.Daneel.Converters;
using Toggl.Daneel.Extensions;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel
{
    public partial class RatingView : MvxView
    {
        public IMvxCommand CTATappedCommand { get; set; }
        public IMvxCommand DismissTappedCommand { get; set; }
        public IMvxCommand<bool> ImpressionTappedCommand { get; set; }

        public RatingView (IntPtr handle) : base (handle)
        {
        }

        public static RatingView Create()
        {
            var arr = NSBundle.MainBundle.LoadNib(nameof(RatingView), null, null);
            return Runtime.GetNSObject<RatingView>(arr.ValueAt(0));
        }

        public override void MovedToSuperview()
        {
            base.MovedToSuperview();

            var descriptionStringAttributes = new UIStringAttributes
            {
                ParagraphStyle = new NSMutableParagraphStyle
                {
                    MaximumLineHeight = 22,
                    MinimumLineHeight = 22,
                    Alignment = UITextAlignment.Center
                }
            };
            var descriptionStringConverter = new StringToAttributedStringValueConverter(descriptionStringAttributes);
            var inverseBoolConverter = new BoolToConstantValueConverter<bool>(false, true);
            var boolToHeightConverter = new BoolToConstantValueConverter<nfloat>(289, 262);
            var bindingSet = this.CreateBindingSet<RatingView, RatingViewModel>();

            TranslatesAutoresizingMaskIntoConstraints = false;
            var heightConstraint = HeightAnchor.ConstraintEqualTo(1);
            heightConstraint.Active = true;

            //Text
            bindingSet.Bind(CtaTitle).To(vm => vm.CtaTitle);

            bindingSet.Bind(CtaDescription)
                      .For(v => v.AttributedText)
                      .To(vm => vm.CtaDescription)
                      .WithConversion(descriptionStringConverter);
            
            bindingSet.Bind(CtaButton)
                      .For(v => v.BindTitle())
                      .To(vm => vm.CtaButtonTitle);

            //Visibility
            bindingSet.Bind(QuestionView)
                      .For(v => v.BindVisibilityWithFade())
                      .To(vm => vm.GotImpression)
                      .WithConversion(inverseBoolConverter);

            bindingSet.Bind(CtaView)
                      .For(v => v.BindVisibilityWithFade())
                      .To(vm => vm.GotImpression);

            //Commands
            bindingSet.Bind(CtaButton).To(vm => vm.PerformMainAction);
            bindingSet.Bind(DismissButton).To(vm => vm.DismissViewCommand);

            bindingSet.Bind(YesView)
                      .For(v => v.BindTap())
                      .To(vm => vm.RegisterImpressionCommand)
                      .CommandParameter(true);

            bindingSet.Bind(NotReallyView)
                      .For(v => v.BindTap())
                      .To(vm => vm.RegisterImpressionCommand)
                      .CommandParameter(false);

            //Size
            bindingSet.Bind(heightConstraint)
                      .For(v => v.BindAnimatedConstant())
                      .To(vm => vm.GotImpression)
                      .WithConversion(boolToHeightConverter);

            bindingSet.Apply();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            SetupAsCard(QuestionView);
            SetupAsCard(CtaView);

            CtaButton.Layer.CornerRadius = 8;
            CtaView.Layer.MasksToBounds = false;
        }

        private void SetupAsCard(UIView view)
        {
            var shadowPath = UIBezierPath.FromRect(view.Bounds);
            view.Layer.ShadowPath?.Dispose();
            view.Layer.ShadowPath = shadowPath.CGPath;

            view.Layer.CornerRadius = 8;
            view.Layer.ShadowRadius = 4;
            view.Layer.ShadowOpacity = 0.1f;
            view.Layer.MasksToBounds = false;
            view.Layer.ShadowOffset = new CGSize(0, 2);
            view.Layer.ShadowColor = UIColor.Black.CGColor;
        }
    }
}
