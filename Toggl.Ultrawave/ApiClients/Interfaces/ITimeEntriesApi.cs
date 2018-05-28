using Toggl.Multivac.Models;
using Toggl.Ultrawave.ApiClients.Interfaces;

namespace Toggl.Ultrawave.ApiClients
{
    public interface ITimeEntriesApi
        : IDeletingApiClient<ITimeEntry>,
          ICreatingApiClient<ITimeEntry>,
          IUpdatingApiClient<ITimeEntry>,
          IPullingApiClient<ITimeEntry>,
          IPullingChangedApiClient<ITimeEntry>
    {
    }
}
