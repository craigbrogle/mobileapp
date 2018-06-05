namespace Toggl.Multivac
{
    public struct RatingViewConfiguration
    {
        public int DayCount { get; }

        public RatingViewCriterion Criterion { get; }

        public RatingViewConfiguration(int dayCount, string criterion)
        {
            DayCount = dayCount;
            switch (criterion)
            {
                case "stop":
                    Criterion = RatingViewCriterion.Stop;
                    break;
                case "start":
                    Criterion = RatingViewCriterion.Start;
                    break;
                case "continue":
                    Criterion = RatingViewCriterion.Continue;
                    break;
                default:
                    Criterion = RatingViewCriterion.None;
                    break;
            }
        }
    }

    public enum RatingViewCriterion
    {
        None,
        Stop,
        Start,
        Continue
    }
}
