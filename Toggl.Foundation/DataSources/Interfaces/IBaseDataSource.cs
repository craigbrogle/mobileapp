using System;
using System.Collections.Generic;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.DataSources.Interfaces
{
    public interface IBaseDataSource<TThreadsafe, TDatabase>
        where TThreadsafe : IThreadSafeModel, TDatabase
    {
        IObservable<TThreadsafe> Create(TThreadsafe entity);

        IObservable<TThreadsafe> Update(TThreadsafe entity);

        IObservable<TThreadsafe> Overwrite(TThreadsafe original, TThreadsafe entity);

        IObservable<IConflictResolutionResult<TThreadsafe>> OverwriteIfOriginalDidNotChange(TThreadsafe original, TThreadsafe entity);

        IObservable<IEnumerable<IConflictResolutionResult<TThreadsafe>>> BatchUpdate(IEnumerable<TThreadsafe> entities);
    }
}
