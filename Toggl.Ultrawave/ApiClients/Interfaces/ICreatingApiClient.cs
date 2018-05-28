using System;

namespace Toggl.Ultrawave.ApiClients
{
    public interface ICreatingApiClient<T>
    {
        IObservable<T> Create(T entity);
    }
}
