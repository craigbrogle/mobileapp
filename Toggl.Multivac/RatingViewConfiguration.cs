namespace Toggl.Multivac
{
    public struct RatingViewConfiguration
    {
        public int DayCount { get; }

        public string Criterion { get; }

        public RatingViewConfiguration(int dayCount, string criterion)
        {
            DayCount = dayCount;
            Criterion = criterion;
        }
    }
}
