using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.DataSources
{
    public abstract class SingletonDataSource<TThreadsafe, TDatabase>
        : BaseDataSource<TThreadsafe, TDatabase>,
          ISingletonDataSource<TThreadsafe, TDatabase>
        where TDatabase : IDatabaseSyncable
        where TThreadsafe : IThreadSafeModel, IIdentifiable, TDatabase
    {
        private readonly ISingleObjectStorage<TDatabase> storage;

        private readonly ISubject<TThreadsafe> currentSubject;

        private IDisposable initializationDisposable;

        public IObservable<TThreadsafe> Current { get; }

        public SingletonDataSource(ISingleObjectStorage<TDatabase> storage, TThreadsafe defaultCurrentValue)
            : base(storage)
        {
            Ensure.Argument.IsNotNull(storage, nameof(storage));

            this.storage = storage;

            currentSubject = new BehaviorSubject<TThreadsafe>(defaultCurrentValue);

            Current = currentSubject.AsObservable();

            // right after login/signup the database does not contain the preferences and retreiving
            // it will fail we can ignore this error because it will be immediately fetched and until
            // then the default preferences will be used
            initializationDisposable = storage.Single()
                .Select(Convert)
                .Subscribe(currentSubject.OnNext, (Exception _) => { });
        }

        public virtual IObservable<TThreadsafe> Get()
            => storage.Single().Select(Convert);

        public override IObservable<TThreadsafe> Create(TThreadsafe entity)
            => base.Create(entity).Do(currentSubject.OnNext);

        public override IObservable<TThreadsafe> Update(TThreadsafe entity)
            => base.Update(entity).Do(currentSubject.OnNext);

        public override IObservable<TThreadsafe> Overwrite(TThreadsafe original, TThreadsafe entity)
            => base.Overwrite(original, entity).Do(currentSubject.OnNext);

        public override IObservable<IConflictResolutionResult<TThreadsafe>> OverwriteIfOriginalDidNotChange(
            TThreadsafe original, TThreadsafe entity)
            => base.OverwriteIfOriginalDidNotChange(original, entity).Do(handleConflictResolutionResult);

        private void handleConflictResolutionResult(IConflictResolutionResult<TThreadsafe> result)
        {
            switch (result)
            {
                case CreateResult<TThreadsafe> c:
                    currentSubject.OnNext(c.Entity);
                    break;

                case UpdateResult<TThreadsafe> u:
                    currentSubject.OnNext(u.Entity);
                    break;
            }
        }
    }
}
