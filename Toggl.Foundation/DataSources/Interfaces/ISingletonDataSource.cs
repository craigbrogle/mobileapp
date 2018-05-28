using System;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.DataSources.Interfaces
{
    public interface ISingletonDataSource<TThreadsafe, TDatabase> : IBaseDataSource<TThreadsafe, TDatabase>
        where TDatabase : IDatabaseSyncable
        where TThreadsafe : IThreadSafeModel, TDatabase
    {
        IObservable<TThreadsafe> Current { get; }

        IObservable<TThreadsafe> Get();
    }
}
