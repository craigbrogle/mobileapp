using System;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Tests.Mocks
{
    public sealed class MockPreferences : IThreadSafePreferences
    {
        public TimeFormat TimeOfDayFormat { get; set; }

        public DateFormat DateFormat { get; set; }

        public DurationFormat DurationFormat { get; set; }

        public bool CollapseTimeEntries { get; set; }

        public SyncStatus SyncStatus { get; set; }

        public string LastSyncErrorMessage { get; set; }

        public bool IsDeleted { get; set; }

        public long Id { get; set; }
    }
}
