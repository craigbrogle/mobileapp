using System;

namespace Toggl.Foundation.Sync.States.Push.Interfaces
{
    public interface IPushState<T>
    {   
        StateResult<T> PushEntity { get; }

        StateResult NothingToPush { get; }

        IObservable<ITransition> Start();
    }
}
