namespace Toggl.Foundation.MvvmCross.Parameters
{
    public sealed class EditDurationParameters
    {
        public enum InitialFocus
        {
            Duration,
            None
        }

        public DurationParameter DurationParam { get; }
        public bool IsDurationInitiallyFocused { get; }

        public EditDurationParameters(DurationParameter durationParam,
            bool isDurationInitiallyFocused = false)
        {
            DurationParam = durationParam;
            IsDurationInitiallyFocused = isDurationInitiallyFocused;
        }
    }
}
