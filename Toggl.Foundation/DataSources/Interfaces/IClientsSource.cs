using System;
using System.Collections.Generic;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public interface IClientsSource : IDataSource<IThreadSafeClient, IDatabaseClient>
    {
        IObservable<IThreadSafeClient> Create(string name, long workspaceId);

        IObservable<IEnumerable<IThreadSafeClient>> GetAllInWorkspace(long workspaceId);
    }
}