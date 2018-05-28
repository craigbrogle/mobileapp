using System;
using System.Collections.Generic;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public sealed class ClientsDataSource
        : DataSource<IThreadSafeClient, IDatabaseClient>, IClientsSource
    {
        private readonly IIdProvider idProvider;
        private readonly ITimeService timeService;

        public ClientsDataSource(IIdProvider idProvider, IRepository<IDatabaseClient> repository, ITimeService timeService)
            : base(repository)
        {
            Ensure.Argument.IsNotNull(idProvider, nameof(idProvider));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));

            this.idProvider = idProvider;
            this.timeService = timeService;
        }

        public IObservable<IThreadSafeClient> Create(string name, long workspaceId)
            => idProvider.GetNextIdentifier()
                .Apply(Client.Builder.Create)
                .SetName(name)
                .SetWorkspaceId(workspaceId)
                .SetAt(timeService.CurrentDateTime)
                .SetSyncStatus(SyncStatus.SyncNeeded)
                .Build()
                .Apply(Create);

        public IObservable<IEnumerable<IThreadSafeClient>> GetAllInWorkspace(long workspaceId)
            => GetAll(c => c.WorkspaceId == workspaceId);

        protected override IThreadSafeClient Convert(IDatabaseClient entity)
            => Client.From(entity);

        protected override ConflictResolutionMode ResolveConflicts(IDatabaseClient first, IDatabaseClient second)
            => Resolver.ForClients.Resolve(first, second);
    }
}
