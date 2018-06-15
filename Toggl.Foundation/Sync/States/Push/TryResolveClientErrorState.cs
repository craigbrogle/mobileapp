using System;
using System.Reactive.Linq;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States.Push
{
    public sealed class TryResolveClientErrorState<T> : ISyncState<(Exception Error, T Entity)>
        where T : class, IThreadSafeModel
    {
        public StateResult UnresolvedTooManyRequests { get; } = new StateResult();

        public StateResult<(Exception, T)> Unresolved { get; } = new StateResult<(Exception, T)>();

        public IObservable<ITransition> Start((Exception Error, T Entity) parameter)
            => parameter.Error is ClientErrorException == false
                ? Observable.Throw<ITransition>(new ArgumentException(nameof(parameter.Error)))
                : parameter.Error is TooManyRequestsException
                    ? Observable.Return((ITransition)UnresolvedTooManyRequests.Transition())
                    : Observable.Return(Unresolved.Transition(parameter));
    }
}
