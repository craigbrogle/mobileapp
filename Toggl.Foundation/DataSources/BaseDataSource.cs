using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public abstract class BaseDataSource<T, U> : IBaseDataSource<T, U>
        where U : IDatabaseModel
        where T : IThreadSafeModel, IIdentifiable, U
    {
        protected readonly IRepository<U> Repository;

        protected virtual IRivalsResolver<U> RivalsResolver { get; } = null;

        protected BaseDataSource(IRepository<U> repository)
        {
            Ensure.Argument.IsNotNull(repository, nameof(repository));

            Repository = repository;
        }

        public virtual IObservable<T> Create(T entity)
            => Repository.Create(entity).Select(Convert);

        public virtual IObservable<T> Update(T entity)
            => Repository.Update(entity.Id, entity).Select(Convert);
        
        public virtual IObservable<T> Overwrite(T original, T entity)
            => Repository.Update(original.Id, entity).Select(Convert);

        public virtual IObservable<IConflictResolutionResult<T>> OverwriteIfOriginalDidNotChange(T original, T entity)
            => Repository.UpdateWithConflictResolution(original.Id, entity, ignoreIfChangedLocally(original), RivalsResolver)
                .Select(result => result.ToThreadSafeResult(Convert));

        public virtual IObservable<IEnumerable<IConflictResolutionResult<T>>> BatchUpdate(IEnumerable<T> entities)
            => Repository.BatchUpdate(
                    convertEntitiesForBatchUpdate(entities),
                    ResolveConflicts,
                    RivalsResolver)
                .ToThreadSafeResult(Convert);

        private Func<U, U, ConflictResolutionMode> ignoreIfChangedLocally(T localEntity)
            => (currentLocal, serverEntity) => localEntity.DiffersFrom(currentLocal)
                ? ConflictResolutionMode.Ignore
                : ConflictResolutionMode.Update;

        private IEnumerable<(long, U)> convertEntitiesForBatchUpdate(
            IEnumerable<T> entities)
            => entities.Select(entity => (entity.Id, (U)entity));

        protected abstract T Convert(U entity);

        protected abstract ConflictResolutionMode ResolveConflicts(U first, U second);
    }
}
