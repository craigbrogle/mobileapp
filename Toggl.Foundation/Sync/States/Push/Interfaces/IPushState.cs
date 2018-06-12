using System;

namespace Toggl.Foundation.Sync.States.Push.Interfaces
{
    public interface IPushState<T> : ISyncState
    {
        StateResult<T> PushEntity { get; }

        StateResult NothingToPush { get; }
    }
}
