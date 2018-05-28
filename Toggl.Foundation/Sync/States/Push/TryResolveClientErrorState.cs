using System;
using System.Reactive.Linq;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States.Push
{
    public sealed class TryResolveClientErrorState<TThreadsafe>
        where TThreadsafe : class, IThreadSafeModel
    {
        public StateResult UnresolvedTooManyRequests { get; } = new StateResult();

        public StateResult<(Exception, TThreadsafe)> Unresolved { get; } = new StateResult<(Exception, TThreadsafe)>();

        public IObservable<ITransition> Start((Exception Error, TThreadsafe Entity) parameter)
            => parameter.Error is ClientErrorException == false
                ? Observable.Throw<ITransition>(new ArgumentException(nameof(parameter.Error)))
                : parameter.Error is TooManyRequestsException
                    ? Observable.Return((ITransition)UnresolvedTooManyRequests.Transition())
                    : Observable.Return(Unresolved.Transition(parameter));
    }
}
