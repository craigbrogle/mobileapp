using Toggl.Foundation.Analytics;
using Toggl.Foundation.MvvmCross.Parameters;

namespace Toggl.Foundation.MvvmCross.ViewModels.Calendar.QuickSelectShortcuts
{
    public sealed class CalendarTodayQuickSelectShortcut : CalendarBaseQuickSelectShortcut
    {
        public CalendarTodayQuickSelectShortcut(ITimeService timeService)
            : base(timeService, Resources.Today)
        {
        }

        public override DateRangeParameter GetDateRange()
        {
            var today = TimeService.CurrentDateTime.Date;
            return DateRangeParameter
                .WithDates(today, today)
                .WithSource(ReportsSource.ShortcutToday);
        }
    }
}
