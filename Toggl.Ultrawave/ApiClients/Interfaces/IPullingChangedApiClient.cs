using System;
using System.Collections.Generic;

namespace Toggl.Ultrawave.ApiClients
{
    public interface IPullingChangedApiClient<T>
    {
        IObservable<List<T>> GetAllSince(DateTimeOffset threshold);
    }
}
