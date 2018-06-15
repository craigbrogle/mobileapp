using MvvmCross.Core.ViewModels;

namespace Toggl.Foundation.MvvmCross.ViewModels.Hints
{
    public sealed class ToggleRatingViewVisibilityHint : MvxPresentationHint
    {
        public bool ForceHide { get; }

        public ToggleRatingViewVisibilityHint(bool forceHide = false)
        {
            ForceHide = forceHide;
        }
    }
}
