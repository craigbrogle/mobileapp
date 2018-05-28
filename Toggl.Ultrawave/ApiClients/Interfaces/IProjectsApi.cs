using System;
using System.Collections.Generic;
using Toggl.Multivac.Models;

namespace Toggl.Ultrawave.ApiClients
{
    public interface IProjectsApi
        : IPullingApiClient<IProject>,
          IPullingChangedApiClient<IProject>,
          ICreatingApiClient<IProject>
    {
        IObservable<List<IProject>> Search(long workspaceId, long[] projectIds);
    }
}
