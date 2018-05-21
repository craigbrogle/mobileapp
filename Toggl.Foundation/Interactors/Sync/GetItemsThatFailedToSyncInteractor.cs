using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.Foundation.Models;

namespace Toggl.Foundation.Interactors
{
    public class GetItemsThatFailedToSyncInteractor : IInteractor<IObservable<IEnumerable<SyncFailureItem>>>
    {
        private readonly ITogglDatabase database;

        public GetItemsThatFailedToSyncInteractor(ITogglDatabase database)
        {
            Ensure.Argument.IsNotNull(database, nameof(database));

            this.database = database;
        }

        public IObservable<IEnumerable<SyncFailureItem>> Execute()
        {
            return Observable
                .Concat (
                    getUnsyncedItems(database.Clients),
                    getUnsyncedItems(database.Projects),
                    getUnsyncedItems(database.Tags)
                )
                .ToList();
        }

        private IObservable<SyncFailureItem> getUnsyncedItems<T>(IRepository<T> repository) where T : IDatabaseSyncable
        {
            return repository
                .GetAll(p => p.SyncStatus == SyncStatus.SyncFailed)
                .SelectMany(convertToSyncFailures);
        }

        private IEnumerable<SyncFailureItem> convertToSyncFailures<T>(IEnumerable<T> items) where T : IDatabaseSyncable
        {
            return items.Select( i => new SyncFailureItem(i));
        }
    }
}
