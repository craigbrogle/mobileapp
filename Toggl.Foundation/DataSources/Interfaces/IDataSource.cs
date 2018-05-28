using System;
using System.Collections.Generic;
using System.Reactive;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources.Interfaces
{
    public interface IDataSource<TThreadsafe, TDatabase> : IBaseDataSource<TThreadsafe, TDatabase>
        where TDatabase : IDatabaseModel
        where TThreadsafe : TDatabase, IThreadSafeModel
    {
        IObservable<TThreadsafe> GetById(long id);

        IObservable<IEnumerable<TThreadsafe>> GetAll();

        IObservable<IEnumerable<TThreadsafe>> GetAll(Func<TDatabase, bool> predicate);

        IObservable<Unit> Delete(long id);
    }
}
