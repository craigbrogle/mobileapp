using System;
using CoreGraphics;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.Binding.iOS.Views;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Daneel.Converters;
using Toggl.Daneel.Extensions;
using Toggl.Foundation.MvvmCross.Combiners;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.Views.Reports
{
    public partial class ReportsHeaderView : MvxTableViewHeaderFooterView
    {
        public static readonly NSString Key = new NSString(nameof(ReportsHeaderView));
        public static readonly UINib Nib;

        static ReportsHeaderView()
        {
            Nib = UINib.FromName(nameof(ReportsHeaderView), NSBundle.MainBundle);
        }

        public ReportsHeaderView(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            var templateImage = TotalDurationGraph.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            TotalDurationGraph.Image = templateImage;

            prepareViews();

            var colorConverter = new BoolToConstantValueConverter<UIColor>(
                Color.Reports.Disabled.ToNativeColor(),
                Color.Reports.TotalTimeActivated.ToNativeColor()
            );

            var durationCombiner = new DurationValueCombiner();

            this.DelayBind(() =>
            {
                var bindingSet = this.CreateBindingSet<ReportsHeaderView, ReportsViewModel>();

                //Text
                bindingSet.Bind(BillablePercentageLabel)
                          .For(v => v.AttributedText)
                          .To(vm => vm.BillablePercentage)
                          .WithConversion(new ReportPercentageLabelValueConverter());

                bindingSet.Bind(TotalDurationLabel)
                          .For(v => v.Text)
                          .ByCombining(durationCombiner,
                              vm => vm.TotalTime,
                              vm => vm.DurationFormat);

                //Loading chart
                bindingSet.Bind(LoadingPieChartView)
                          .For(v => v.BindVisibilityWithFade())
                          .To(vm => vm.IsLoading);

                bindingSet.Bind(LoadingCardView)
                          .For(v => v.BindVisibilityWithFade())
                          .To(vm => vm.IsLoading);

                //Pretty stuff
                bindingSet.Bind(PieChartView)
                          .For(v => v.Segments)
                          .To(vm => vm.GroupedSegments);

                bindingSet.Bind(BillablePercentageView)
                          .For(v => v.Percentage)
                          .To(vm => vm.BillablePercentage);

                bindingSet.Bind(TotalDurationGraph)
                          .For(v => v.TintColor)
                          .To(vm => vm.TotalTimeIsZero)
                          .WithConversion(colorConverter);

                bindingSet.Bind(TotalDurationLabel)
                          .For(v => v.TextColor)
                          .To(vm => vm.TotalTimeIsZero)
                          .WithConversion(colorConverter);

                //Visibility
                bindingSet.Bind(EmptyStateView)
                          .For(v => v.BindVisible())
                          .To(vm => vm.ShowEmptyState);

                bindingSet.Apply();
            });
        }

        private void prepareViews()
        {
            prepareCard(OverviewCardView);
            prepareCard(LoadingCardView);

            TotalTitleLabel.SetKerning(-0.2);
            TotalDurationLabel.SetKerning(-0.2);
            BillableTitleLabel.SetKerning(-0.2);
            BillablePercentageLabel.SetKerning(-0.2);
        }

        private void prepareCard(UIView view)
        {
            view.Layer.CornerRadius = 8;
            view.Layer.ShadowColor = UIColor.Black.CGColor;
            view.Layer.ShadowRadius = 16;
            view.Layer.ShadowOffset = new CGSize(0, 2);
            view.Layer.ShadowOpacity = 0.1f;
        }
    }
}
