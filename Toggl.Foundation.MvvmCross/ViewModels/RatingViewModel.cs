using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class RatingViewModel : MvxViewModel
    {
        private readonly ITogglDataSource dataSource;
        private readonly IAnalyticsService analyticsService;

        public bool GotImpression { get; private set; }

        public string CTATitle { get; private set; }

        public string CTADescription { get; private set; }

        public string CTAButtonTitle { get; set; }

        public MvxCommand<bool> RegisterImpressionCommand { get; set; }
        public MvxCommand LeaveReviewCommand { get; set; }
        public MvxCommand DismissViewCommand { get; set; }

        public RatingViewModel(ITogglDataSource dataSource, IAnalyticsService analyticsService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.dataSource = dataSource;
            this.analyticsService = analyticsService;

            GotImpression = false;

            RegisterImpressionCommand = new MvxCommand<bool>(registerImpression);
            LeaveReviewCommand = new MvxCommand(leaveReview);
            DismissViewCommand = new MvxCommand(dismiss);
        }

        public async override Task Initialize()
        {
            await base.Initialize();
        }

        private void registerImpression(bool isPositive)
        {
            GotImpression = true;
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

        private void leaveReview()
        {
            GotImpression = false;
        }

        private void dismiss()
        {
            GotImpression = true;
        }
    }
}
