using System;
using System.Reactive;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public interface ITimeEntriesSource
        : IObservableDataSource<IThreadSafeTimeEntry, IDatabaseTimeEntry>
    {
        IObservable<IThreadSafeTimeEntry> CurrentlyRunningTimeEntry { get; }

        IObservable<bool> IsEmpty { get; }

        IObservable<Unit> SoftDelete(IThreadSafeTimeEntry timeEntry);

        IObservable<IThreadSafeTimeEntry> Stop(DateTimeOffset stopTime);

        IObservable<IThreadSafeTimeEntry> Update(EditTimeEntryDto dto);
    }
}
