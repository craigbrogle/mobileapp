using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.MvvmCross.ViewModels.Hints;
using Toggl.Foundation.Services;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class RatingViewModel : MvxViewModel
    {
        private readonly ITogglDataSource dataSource;
        private readonly IRatingService ratingService;
        private readonly IFeedbackService feedbackService;
        private readonly IAnalyticsService analyticsService;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IMvxNavigationService navigationService;

        private bool? impressionIsPositive;

        public bool GotImpression { get; private set; } = false;

        public string CtaTitle { get; private set; } = "";

        public string CtaDescription { get; private set; } = "";

        public string CtaButtonTitle { get; private set; } = "";

        public IMvxCommand<bool> RegisterImpressionCommand { get; private set; }

        public IMvxAsyncCommand PerformMainAction { get; private set; }

        public IMvxCommand DismissViewCommand { get; private set; }

        public RatingViewModel(
            ITogglDataSource dataSource,
            IRatingService ratingService,
            IFeedbackService feedbackService,
            IAnalyticsService analyticsService,
            IOnboardingStorage onboardingStorage,
            IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(ratingService, nameof(ratingService));
            Ensure.Argument.IsNotNull(feedbackService, nameof(feedbackService));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.dataSource = dataSource;
            this.ratingService = ratingService;
            this.feedbackService = feedbackService;
            this.analyticsService = analyticsService;
            this.onboardingStorage = onboardingStorage;
            this.navigationService = navigationService;

            DismissViewCommand = new MvxCommand(dismiss);
            PerformMainAction = new MvxAsyncCommand(performMainAction);
            RegisterImpressionCommand = new MvxCommand<bool>(registerImpression);
        }

        private void registerImpression(bool isPositive)
        {
            GotImpression = true;
            impressionIsPositive = isPositive;
            analyticsService.UserFinishedRatingViewFirstStep.Track(isPositive);
            if (isPositive)
            {
                CtaTitle = Resources.RatingViewPositiveCTATitle;
                CtaDescription = Resources.RatingViewPositiveCTADescription;
                CtaButtonTitle = Resources.RatingViewPositiveCTAButtonTitle;
                onboardingStorage.SetRatingViewOutcome(RatingViewOutcome.PositiveImpression);
            }
            else
            {
                CtaTitle = Resources.RatingViewNegativeCTATitle;
                CtaDescription = Resources.RatingViewNegativeCTADescription;
                CtaButtonTitle = Resources.RatingViewNegativeCTAButtonTitle;
                onboardingStorage.SetRatingViewOutcome(RatingViewOutcome.NegativeImpression);
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
                onboardingStorage.SetRatingViewOutcome(RatingViewOutcome.AppWasRated);
            }
            else
            {
                await feedbackService.SubmitFeedback();
                analyticsService.UserFinishedRatingViewSecondStep.Track(RatingViewSecondStepOutcome.FeedbackWasLeft);
                onboardingStorage.SetRatingViewOutcome(RatingViewOutcome.FeedbackWasLeft);
            }
        }

        private void dismiss()
        {
            navigationService.ChangePresentation(
                new ToggleRatingViewVisibilityHint(forceHide: true)
            );

            if (impressionIsPositive.HasValue)
            {
                if (impressionIsPositive.Value)
                {
                    onboardingStorage.SetRatingViewOutcome(RatingViewOutcome.AppWasNotRated);
                    analyticsService.UserFinishedRatingViewSecondStep.Track(RatingViewSecondStepOutcome.AppWasNotRated);
                }
                else
                {
                    onboardingStorage.SetRatingViewOutcome(RatingViewOutcome.FeedbackWasNotLeft);
                    analyticsService.UserFinishedRatingViewSecondStep.Track(RatingViewSecondStepOutcome.FeedbackWasNotLeft);
                }
            }
        }
    }
}
