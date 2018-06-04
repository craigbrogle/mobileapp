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
    public abstract class BaseDataSource<TThreadsafe, TDatabase> : IBaseDataSource<TThreadsafe>
        where TDatabase : IDatabaseModel
        where TThreadsafe : IThreadSafeModel, IIdentifiable, TDatabase
    {
        protected readonly IRepository<TDatabase> Repository;

        protected virtual IRivalsResolver<TDatabase> RivalsResolver { get; } = null;

        protected BaseDataSource(IRepository<TDatabase> repository)
        {
            Ensure.Argument.IsNotNull(repository, nameof(repository));

            Repository = repository;
        }

        public virtual IObservable<TThreadsafe> Create(TThreadsafe entity)
            => Repository.Create(entity).Select(Convert);

        public virtual IObservable<TThreadsafe> Update(TThreadsafe entity)
            => Repository.Update(entity.Id, entity).Select(Convert);

        public virtual IObservable<TThreadsafe> Overwrite(TThreadsafe original, TThreadsafe entity)
            => Repository.Update(original.Id, entity).Select(Convert);

        public virtual IObservable<IConflictResolutionResult<TThreadsafe>> OverwriteIfOriginalDidNotChange(TThreadsafe original, TThreadsafe entity)
            => Repository.UpdateWithConflictResolution(original.Id, entity, ignoreIfChangedLocally(original), RivalsResolver)
                .Select(result => result.ToThreadSafeResult(Convert));

        private Func<TDatabase, TDatabase, ConflictResolutionMode> ignoreIfChangedLocally(TThreadsafe localEntity)
            => (currentLocal, serverEntity) => localEntity.DiffersFrom(currentLocal)
                ? ConflictResolutionMode.Ignore
                : ConflictResolutionMode.Update;

        protected abstract TThreadsafe Convert(TDatabase entity);

        protected abstract ConflictResolutionMode ResolveConflicts(TDatabase first, TDatabase second);
    }
}
