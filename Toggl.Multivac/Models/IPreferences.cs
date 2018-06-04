﻿namespace Toggl.Multivac.Models
{
    public interface IPreferences
    {
        TimeFormat TimeOfDayFormat { get; }

        DateFormat DateFormat { get; }

        DurationFormat DurationFormat { get; }

        bool CollapseTimeEntries { get; }
    }
}
