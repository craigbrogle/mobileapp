using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;

namespace Toggl.Foundation.Interactors
{
    public class GetItemsThatFailedToSyncInteractor : IInteractor<IObservable<IEnumerable<SyncFailureItem>>>
    {
        private readonly ITogglDataSource dataSource;

        public GetItemsThatFailedToSyncInteractor(ITogglDataSource dataSource)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.dataSource = dataSource;
        }

        public IObservable<IEnumerable<SyncFailureItem>> Execute()
        {
            return Observable
                .Concat (
                    getUnsyncedItems(dataSource.Clients),
                    getUnsyncedItems(dataSource.Projects),
                    getUnsyncedItems(dataSource.Tags)
                )
                .ToList();
        }

        private IObservable<SyncFailureItem> getUnsyncedItems<TThreadsafe, TDatabase>(
            IDataSource<TThreadsafe, TDatabase> source)
            where TDatabase : IDatabaseSyncable
            where TThreadsafe : TDatabase, IThreadSafeModel
        {
            return source
                .GetAll(p => p.SyncStatus == SyncStatus.SyncFailed)
                .SelectMany(convertToSyncFailures);
        }

        private IEnumerable<SyncFailureItem> convertToSyncFailures<T>(IEnumerable<T> items)
            where T : IThreadSafeModel, IDatabaseSyncable
        {
            return items.Select( i => new SyncFailureItem(i));
        }
    }
}
