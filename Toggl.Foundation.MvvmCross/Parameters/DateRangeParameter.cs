using System;
using Toggl.Foundation.Analytics;

namespace Toggl.Foundation.MvvmCross.Parameters
{
    public sealed class DateRangeParameter
    {
        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public ReportsSource Source { get; set; }

        public static DateRangeParameter WithDates(
            DateTimeOffset start,
            DateTimeOffset end
        )
        {
            if (start > end)
                (start, end) = (end, start);

            return new DateRangeParameter { StartDate = start, EndDate = end, Source = ReportsSource.Other };
        }

        public DateRangeParameter WithSource(ReportsSource source)
        {
            return new DateRangeParameter { StartDate = this.StartDate, EndDate = this.EndDate, Source = source };
        }
    }
}
