using System;
using System.Collections.Generic;
using Toggl.PrimeRadiant.Models;
using Toggl.Foundation.Models;
     
namespace Toggl.Foundation.Interactors
{
    public sealed partial class InteractorFactory : IInteractorFactory
    {
        public IInteractor<IObservable<int>> GetNumberOfItemsThatFailedToSync()
            => new GetNumberOfItemsThatFailedToSyncInteractor(database);

        public IInteractor<IObservable<IEnumerable<IDatabaseProject>>> GetProjectsThatFailedToSync()
            => new GetItemsThatFailedToSyncInteractor<IDatabaseProject>(database.Projects, Project.From);

        public IInteractor<IObservable<IEnumerable<IDatabaseClient>>> GetClientsThatFailedToSync()
            => new GetItemsThatFailedToSyncInteractor<IDatabaseClient>(database.Clients, Client.From);

        public IInteractor<IObservable<IEnumerable<IDatabaseTag>>> GetTagsThatFailedToSync()
          => new GetItemsThatFailedToSyncInteractor<IDatabaseTag>(database.Tags, Tag.From);
    }
}

