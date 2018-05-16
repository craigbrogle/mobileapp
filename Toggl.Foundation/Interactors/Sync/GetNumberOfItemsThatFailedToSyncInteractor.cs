using System;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;
using Toggl.Multivac;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Interactors
{
    public class GetNumberOfItemsThatFailedToSyncInteractor : IInteractor<IObservable<int>>
    {
        private readonly ITogglDatabase database;

        public GetNumberOfItemsThatFailedToSyncInteractor(ITogglDatabase database)
        {
            Ensure.Argument.IsNotNull(database, nameof(database));

            this.database = database;
        }

        public IObservable<int> Execute()
        {
            var projects = database.Projects.GetAll().Select(countFailedToSync);
            var clients = database.Clients.GetAll().Select(countFailedToSync);
            var tags = database.Tags.GetAll().Select(countFailedToSync);

            return Observable.CombineLatest(
                projects,
                clients,
                tags,
                (projectsCount, clientsCount, tagsCount) => projectsCount + clientsCount + tagsCount);
        }

        private int countFailedToSync(IEnumerable<IDatabaseSyncable> items)
        {
            return items
                .Where(p => p.SyncStatus == SyncStatus.SyncFailed)
                .Count();
        }
    }
}
