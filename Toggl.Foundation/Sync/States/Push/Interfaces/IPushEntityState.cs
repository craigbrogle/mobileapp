using System;
using Toggl.Foundation.Models.Interfaces;

namespace Toggl.Foundation.Sync.States.Push.Interfaces
{
    public interface IPushEntityState<T> : ISyncState<T>
        where T : IThreadSafeModel
    {
        StateResult<(Exception, T)> ServerError { get; }

        StateResult<(Exception, T)> ClientError { get; }

        StateResult<(Exception, T)> UnknownError { get; }
    }
}
