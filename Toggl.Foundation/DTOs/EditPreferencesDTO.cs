using Toggl.Multivac;

namespace Toggl.Foundation.DTOs
{
    public struct EditPreferencesDTO
    {
        public New<DateFormat> DateFormat { get; set; }
        public New<DurationFormat> DurationFormat { get; set; }
        public New<TimeFormat> TimeOfDayFormat { get; set; }
        public New<bool> CollapseTimeEntries { get; set; }
    }
}
