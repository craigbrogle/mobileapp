using System;

namespace Toggl.Foundation.Sync.States.Pull
{
    public interface IPersistState
    {
        StateResult<IFetchObservables> FinishedPersisting { get; }

        IObservable<ITransition> Start(IFetchObservables fetch);
    }
}
