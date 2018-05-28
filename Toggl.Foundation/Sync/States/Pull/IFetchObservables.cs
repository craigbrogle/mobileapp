using System;
using System.Collections.Generic;

namespace Toggl.Foundation.Sync.States
{
    public interface IFetchObservables
    {
        IObservable<List<T>> Get<T>();
    }
}
