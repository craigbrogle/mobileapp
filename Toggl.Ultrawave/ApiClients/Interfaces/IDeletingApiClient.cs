using System;
using System.Reactive;

namespace Toggl.Ultrawave.ApiClients.Interfaces
{
    public interface IDeletingApiClient<T>
    {
        IObservable<Unit> Delete(T entity);
    }
}
