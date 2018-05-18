using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.iOS.Views;
using MvvmCross.iOS.Views.Presenters.Attributes;
using MvvmCross.Plugins.Color;
using MvvmCross.Plugins.Visibility;
using Toggl.Daneel.Extensions;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewControllers
{
    [MvxRootPresentation(WrapInNavigationController = true)]
    public sealed partial class OnboardingViewController : MvxViewController<OnboardingViewModel>
    {
        private readonly TrackPage trackPagePlaceholder = TrackPage.Create();
        private readonly MostUsedPage mostUsedPagePlaceholder = MostUsedPage.Create();
        private readonly ReportsPage reportsPagePlaceholder = ReportsPage.Create();

        public OnboardingViewController() 
            : base(nameof(OnboardingViewController), null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            preparePlaceholders();

            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                var navigationBarHeight = NavigationController.NavigationBar.Frame.Height;
                AdditionalSafeAreaInsets = new UIEdgeInsets(-navigationBarHeight, 0, 0, 0);
            }

            PageControl.Pages = ViewModel.NumberOfPages;
            FirstPageLabel.Text = Resources.OnboardingTrackPageCopy;
            SecondPageLabel.Text = Resources.OnboardingMostUsedPageCopy;
            ThirdPageLabel.Text = Resources.OnboardingReportsPageCopy;

            var visibilityConverter = new MvxVisibilityValueConverter();
            var invertedVisibilityConverter = new MvxInvertedVisibilityValueConverter();
            var colorConverter = new MvxNativeColorValueConverter();
            var bindingSet = this.CreateBindingSet<OnboardingViewController, OnboardingViewModel>();

            var pagedBackgroundImageColorConverter = new PaginationValueConverter<UIImage>(new[]
            {
                UIImage.FromBundle("bgNoiseBlue"),
                UIImage.FromBundle("bgNoisePurple"),
                UIImage.FromBundle("bgNoiseYellow")
            });

            //Commands
            bindingSet.Bind(Skip).To(vm => vm.SkipCommand);
            bindingSet.Bind(Next).To(vm => vm.NextCommand);
            bindingSet.Bind(Previous).To(vm => vm.PreviousCommand);

            //Color
            bindingSet.Bind(View)
                      .For(v => v.BindAnimatedBackground())
                      .To(vm => vm.BackgroundColor)
                      .WithConversion(colorConverter);
            
            bindingSet.Bind(PhoneFrame)
                      .For(v => v.BindAnimatedBackground())
                      .To(vm => vm.BorderColor)
                      .WithConversion(colorConverter);

            //Noise image
            bindingSet.Bind(BackgroundImage)
                      .For(v => v.BindAnimatedImage())
                      .To(vm => vm.CurrentPage)
                      .WithConversion(pagedBackgroundImageColorConverter);

            //Visibility
            bindingSet.Bind(Skip)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.IsLastPage)
                      .WithConversion(invertedVisibilityConverter);

            bindingSet.Bind(Previous)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.IsFirstPage)
                      .WithConversion(invertedVisibilityConverter);

            bindingSet.Bind(trackPagePlaceholder)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.IsTrackPage)
                      .WithConversion(visibilityConverter);

            bindingSet.Bind(mostUsedPagePlaceholder)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.IsMostUsedPage)
                      .WithConversion(visibilityConverter);

            bindingSet.Bind(reportsPagePlaceholder)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.IsSummaryPage)
                      .WithConversion(visibilityConverter);


            //Current Page
            bindingSet.Bind(ScrollView)
                      .For(v => v.BindAnimatedCurrentPage())
                      .To(vm => vm.CurrentPage);

            bindingSet.Bind(PageControl)
                      .For(v => v.CurrentPage)
                      .To(vm => vm.CurrentPage);

            bindingSet.Apply();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.NavigationBar.UserInteractionEnabled = false;
        }

        public override bool PrefersStatusBarHidden()
            => true;

        private void preparePlaceholders()
        {
            PhoneContents.AddSubview(trackPagePlaceholder);
            PhoneContents.AddSubview(mostUsedPagePlaceholder);
            PhoneContents.AddSubview(reportsPagePlaceholder);
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            if (trackPagePlaceholder != null)
                trackPagePlaceholder.Frame = PhoneContents.Bounds;
            if (mostUsedPagePlaceholder != null)
                mostUsedPagePlaceholder.Frame = PhoneContents.Bounds;
            if (reportsPagePlaceholder != null)
                reportsPagePlaceholder.Frame = PhoneContents.Bounds;
        }
    }
}
