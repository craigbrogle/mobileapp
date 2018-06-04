using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public abstract class DataSource<TThreadsafe, TDatabase> : BaseDataSource<TThreadsafe, TDatabase>, IDataSource<TThreadsafe, TDatabase>
        where TDatabase : IDatabaseModel
        where TThreadsafe : TDatabase, IThreadSafeModel, IIdentifiable
    {
        protected DataSource(IRepository<TDatabase> repository)
            : base(repository)
        {
        }

        public IObservable<TThreadsafe> GetById(long id)
            => Repository.GetById(id).Select(Convert);

        public virtual IObservable<IEnumerable<TThreadsafe>> GetAll()
            => Repository.GetAll().Select(entities => entities.Select(Convert));

        public virtual IObservable<IEnumerable<TThreadsafe>> GetAll(Func<TDatabase, bool> predicate)
            => Repository.GetAll(predicate).Select(entities => entities.Select(Convert));

        public virtual IObservable<IEnumerable<IConflictResolutionResult<TThreadsafe>>> DeleteAll(IEnumerable<TThreadsafe> entities)
            => Repository.BatchUpdate(convertEntitiesForBatchUpdate(entities), safeAlwaysDelete)
                         .ToThreadSafeResult(Convert);

        public virtual IObservable<Unit> Delete(long id)
            => Repository.Delete(id);

        public virtual IObservable<IEnumerable<IConflictResolutionResult<TThreadsafe>>> BatchUpdate(IEnumerable<TThreadsafe> entities)
            => Repository.BatchUpdate(
                    convertEntitiesForBatchUpdate(entities),
                    ResolveConflicts,
                    RivalsResolver)
                .ToThreadSafeResult(Convert);

        private IEnumerable<(long, TDatabase)> convertEntitiesForBatchUpdate(
            IEnumerable<TThreadsafe> entities)
            => entities.Select(entity => (entity.Id, (TDatabase)entity));

        private static ConflictResolutionMode safeAlwaysDelete(TDatabase old, TDatabase now)
            => old == null ? ConflictResolutionMode.Ignore : ConflictResolutionMode.Delete;
    }
}
