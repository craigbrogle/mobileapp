using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.States.Push.Interfaces;
using Toggl.Multivac;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Sync.States.Push
{
    internal sealed class PushSingleState<TDatabaseModel, TThreadsafeModel> : IPushState<TThreadsafeModel>
        where TDatabaseModel : IDatabaseSyncable
        where TThreadsafeModel : class, TDatabaseModel, IThreadSafeModel
    {
        private readonly ISingletonDataSource<TThreadsafeModel, TDatabaseModel> dataSource;

        public StateResult<TThreadsafeModel> PushEntity { get; } = new StateResult<TThreadsafeModel>();

        public StateResult NothingToPush { get; } = new StateResult();

        public PushSingleState(ISingletonDataSource<TThreadsafeModel, TDatabaseModel> dataSource)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
        
            this.dataSource = dataSource;
        }

        public IObservable<ITransition> Start() =>
            dataSource
                .Get()
                .Where(entity => entity.SyncStatus == SyncStatus.SyncNeeded)
                .SingleOrDefaultAsync()
                .Select(entity =>
                    entity != null
                        ? (ITransition)PushEntity.Transition(entity)
                        : NothingToPush.Transition());
    }
}
