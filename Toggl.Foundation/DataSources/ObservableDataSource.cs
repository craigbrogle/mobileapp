using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac.Models;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.DataSources
{
    public abstract class ObservableDataSource<TThreadsafe, TDatabase>
        : DataSource<TThreadsafe, TDatabase>, IObservableDataSource<TThreadsafe, TDatabase>
        where TDatabase : IDatabaseSyncable
        where TThreadsafe : IThreadSafeModel, IIdentifiable, TDatabase
    {
        public IObservable<TThreadsafe> Created { get; }

        public IObservable<EntityUpdate<TThreadsafe>> Updated { get; }

        public IObservable<long> Deleted { get; }

        protected readonly Subject<long> DeletedSubject = new Subject<long>();

        protected readonly Subject<TThreadsafe> CreatedSubject = new Subject<TThreadsafe>();

        protected readonly Subject<EntityUpdate<TThreadsafe>> UpdatedSubject = new Subject<EntityUpdate<TThreadsafe>>();

        protected ObservableDataSource(IRepository<TDatabase> repository)
            : base(repository)
        {
            Created = CreatedSubject.AsObservable();
            Updated = UpdatedSubject.AsObservable();
            Deleted = DeletedSubject.AsObservable();
        }

        public override IObservable<TThreadsafe> Create(TThreadsafe entity)
            => base.Create(entity)
                .Do(CreatedSubject.OnNext);

        public override IObservable<TThreadsafe> Update(TThreadsafe entity)
            => base.Update(entity)
                .Do(updatedEntity => UpdatedSubject.OnNext(new EntityUpdate<TThreadsafe>(updatedEntity.Id, updatedEntity)));

        public override IObservable<TThreadsafe> Overwrite(TThreadsafe original, TThreadsafe entity)
            => base.Overwrite(original, entity)
                .Do(updatedEntity => UpdatedSubject.OnNext(new EntityUpdate<TThreadsafe>(original.Id, updatedEntity)));

        public override IObservable<Unit> Delete(long id)
            => base.Delete(id)
                .Do(_ => DeletedSubject.OnNext(id));

        public override IObservable<IConflictResolutionResult<TThreadsafe>> OverwriteIfOriginalDidNotChange(
            TThreadsafe original, TThreadsafe entity)
            => base.OverwriteIfOriginalDidNotChange(original, entity)
                .Do(handleConflictResolutionResult);

        public override IObservable<IEnumerable<IConflictResolutionResult<TThreadsafe>>> BatchUpdate(IEnumerable<TThreadsafe> entities)
            => base.BatchUpdate(entities)
                .Do(updatedEntities => updatedEntities
                    .ForEach(handleConflictResolutionResult));

        private void handleConflictResolutionResult(IConflictResolutionResult<TThreadsafe> result)
        {
            switch (result)
            {
                case DeleteResult<TThreadsafe> d:
                    DeletedSubject.OnNext(d.Id);
                    return;

                case CreateResult<TThreadsafe> c:
                    CreatedSubject.OnNext(c.Entity);
                    return;

                case UpdateResult<TThreadsafe> u:
                    UpdatedSubject.OnNext(new EntityUpdate<TThreadsafe>(u.OriginalId, u.Entity));
                    return;
            }
        }
    }
}
