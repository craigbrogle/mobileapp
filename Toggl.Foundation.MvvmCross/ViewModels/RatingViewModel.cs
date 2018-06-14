using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.MvvmCross.ViewModels.Hints;
using Toggl.Foundation.Services;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class RatingViewModel : MvxViewModel
    {
        private readonly ITogglDataSource dataSource;
        private readonly IRatingService ratingService;
        private readonly IFeedbackService feedbackService;
        private readonly IAnalyticsService analyticsService;
        private readonly IMvxNavigationService navigationService;

        private bool? impressionIsPositive;

        public bool GotImpression { get; private set; }

        public string CTATitle { get; private set; }

        public string CTADescription { get; private set; }

        public string CTAButtonTitle { get; private set; }

        public IMvxCommand<bool> RegisterImpressionCommand { get; private set; }
        public IMvxAsyncCommand PerformMainAction { get; private set; }
        public IMvxCommand DismissViewCommand { get; private set; }

        public RatingViewModel(
            ITogglDataSource dataSource,
            IRatingService ratingService,
            IFeedbackService feedbackService,
            IAnalyticsService analyticsService,
            IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(ratingService, nameof(ratingService));
            Ensure.Argument.IsNotNull(feedbackService, nameof(feedbackService));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.dataSource = dataSource;
            this.ratingService = ratingService;
            this.feedbackService = feedbackService;
            this.analyticsService = analyticsService;
            this.navigationService = navigationService;

            GotImpression = false;

            DismissViewCommand = new MvxCommand(dismiss);
            PerformMainAction = new MvxAsyncCommand(performMainAction);
            RegisterImpressionCommand = new MvxCommand<bool>(registerImpression);
        }

        public async override Task Initialize()
        {
            await base.Initialize();
        }

        private void registerImpression(bool isPositive)
        {
            GotImpression = true;
            impressionIsPositive = isPositive;
            analyticsService.UserFinishedRatingViewFirstStep.Track(isPositive);
            if (isPositive)
            {
                CTATitle = Resources.RatingViewPositiveCTATitle;
                CTADescription = Resources.RatingViewPositiveCTADescription;
                CTAButtonTitle = Resources.RatingViewPositiveCTAButtonTitle;
            }
            else
            {
                CTATitle = Resources.RatingViewNegativeCTATitle;
                CTADescription = Resources.RatingViewNegativeCTADescription;
                CTAButtonTitle = Resources.RatingViewNegativeCTAButtonTitle;
            }
        }

        private async Task performMainAction()
        {
            if (impressionIsPositive == null)
                return;
            if (impressionIsPositive.Value)
            {
                ratingService.AskForRating();
                //We can't really know whether the user actually rated
                //We only know that we presented the iOS rating view
                analyticsService.UserFinishedRatingViewSecondStep.Track(RatingViewSecondStepOutcome.AppWasRated);
            }
            else
            {
                await feedbackService.SubmitFeedback();
                analyticsService.UserFinishedRatingViewSecondStep.Track(RatingViewSecondStepOutcome.FeedbackWasLeft);
            }
        }

        private void dismiss()
        {
            navigationService.ChangePresentation(new ToggleRatingViewVisibilityHint());

            if (impressionIsPositive.HasValue)
            {
                var outcome = impressionIsPositive.Value
                    ? RatingViewSecondStepOutcome.AppWasNotRated
                    : RatingViewSecondStepOutcome.FeedbackWasNotLeft;
                analyticsService.UserFinishedRatingViewSecondStep.Track(outcome);
            }
        }
    }
}
