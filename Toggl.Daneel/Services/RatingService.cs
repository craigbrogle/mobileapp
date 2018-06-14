using StoreKit;
using Toggl.Foundation.Services;

namespace Toggl.Daneel.Services
{
    public sealed class RatingService : IRatingService
    {
        public void AskForRating()
        {
            SKStoreReviewController.RequestReview();
        }
    }
}
