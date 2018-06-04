using System;

namespace Toggl.Ultrawave.ApiClients
{
    public interface IUpdatingApiClient<T>
    {
        IObservable<T> Update(T entity);
    }
}
