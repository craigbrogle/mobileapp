using System;
using Toggl.Multivac.Models;

namespace Toggl.Ultrawave.ApiClients
{
    public interface IWorkspacesApi
        : IPullingApiClient<IWorkspace>
    {
        IObservable<IWorkspace> GetById(long id);
    }
}
