using Toggl.Foundation.Analytics;
using Toggl.Foundation.MvvmCross.Parameters;

namespace Toggl.Foundation.MvvmCross.ViewModels.Calendar.QuickSelectShortcuts
{
    public sealed class CalendarYesterdayQuickSelectShortcut : CalendarBaseQuickSelectShortcut
    {
        public CalendarYesterdayQuickSelectShortcut(ITimeService timeService)
            : base(timeService, Resources.Yesterday)
        {
        }

        public override DateRangeParameter GetDateRange()
        {
            var yesterday = TimeService.CurrentDateTime.Date.AddDays(-1);
            return DateRangeParameter
                .WithDates(yesterday, yesterday)
                .WithSource(ReportsSource.ShortcutYesterday);
        }
    }
}
