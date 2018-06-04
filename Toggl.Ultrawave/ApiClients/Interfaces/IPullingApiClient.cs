using System;
using System.Collections.Generic;

namespace Toggl.Ultrawave.ApiClients
{
    public interface IPullingApiClient<T>
    {
        IObservable<List<T>> GetAll();
    }
}
