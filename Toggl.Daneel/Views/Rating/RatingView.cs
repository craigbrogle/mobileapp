using CoreGraphics;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS.Views;
using MvvmCross.Binding.iOS;
using MvvmCross.Core.ViewModels;
using ObjCRuntime;
using System;
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

            var inverseBoolConverter = new BoolToConstantValueConverter<bool>(false, true);
            var boolToHeightConverter = new BoolToConstantValueConverter<nfloat>(289, 262);
            var bindingSet = this.CreateBindingSet<RatingView, RatingViewModel>();

            TranslatesAutoresizingMaskIntoConstraints = false;
            var heightConstraint = HeightAnchor.ConstraintEqualTo(1);
            heightConstraint.Active = true;

            //bindingSet.Bind(QuestionView)
            //          .For(v => v.BindVisibility())
            //          .To(vm => vm.GotImpression);

            //bindingSet.Bind(CTAView)
            //.For(v => v.BindVisibility())
            //.To(vm => vm.GotImpression)
            //.WithConversion(inverseBoolConverter);

            bindingSet.Bind(heightConstraint)
                      .For(v => v.BindConstant())
                      .To(vm => vm.GotImpression)
                      .WithConversion(boolToHeightConverter);

            bindingSet.Bind(YesView)
                      .For(v => v.BindTap())
                      .To(vm => vm.RegisterImpressionCommand)
                      .CommandParameter(true);

            bindingSet.Apply();

            //YesView.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            //{
            //    ImpressionTappedCommand?.Execute(true);
            //}));

            //NotReallyView.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            //{
            //    ImpressionTappedCommand?.Execute(false);
            //}));

            //CTAButton.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            //{
            //    CTATappedCommand?.Execute();
            //}));

            //DismissButton.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            //{
            //    DismissTappedCommand?.Execute();
            //}));
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var cardViewShadowPath = UIBezierPath.FromRect(CardView.Bounds);
            CardView.Layer.ShadowPath?.Dispose();
            CardView.Layer.ShadowPath = cardViewShadowPath.CGPath;

            CardView.Layer.CornerRadius = 8;
            CardView.Layer.ShadowRadius = 4;
            CardView.Layer.ShadowOpacity = 0.1f;
            CardView.Layer.MasksToBounds = false;
            CardView.Layer.ShadowOffset = new CGSize(0, 2);
            CardView.Layer.ShadowColor = UIColor.Black.CGColor;

            CTAButton.Layer.CornerRadius = 8;
            CTAButton.Layer.MasksToBounds = false;
        }
    }
}
