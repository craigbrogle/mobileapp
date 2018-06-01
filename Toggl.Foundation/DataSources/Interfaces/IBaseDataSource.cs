using System;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.DataSources.Interfaces
{
    public interface IBaseDataSource<T>
        where T : IThreadSafeModel
    {
        IObservable<T> Create(T entity);

        IObservable<T> Update(T entity);

        IObservable<T> Overwrite(T original, T entity);

        IObservable<IConflictResolutionResult<T>> OverwriteIfOriginalDidNotChange(T original, T entity);
    }
}
