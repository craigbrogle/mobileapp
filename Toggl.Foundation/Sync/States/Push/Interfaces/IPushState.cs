using System;

namespace Toggl.Foundation.Sync.States.Push.Interfaces
{
    public interface IPushState<TThreadsafeModel>
    {   
        StateResult<TThreadsafeModel> PushEntity { get; }

        StateResult NothingToPush { get; }

        IObservable<ITransition> Start();
    }
}
