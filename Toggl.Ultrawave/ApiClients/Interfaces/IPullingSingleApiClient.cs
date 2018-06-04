using System;

namespace Toggl.Ultrawave.ApiClients
{
    public interface IPullingSingleApiClient<T>
    {
        IObservable<T> Get();
    }
}
