using System;

namespace Toggl.Foundation.Sync.States.Pull
{
    public interface IPersistState : ISyncState<IFetchObservables>
    {
        StateResult<IFetchObservables> FinishedPersisting { get; }
    }
}
