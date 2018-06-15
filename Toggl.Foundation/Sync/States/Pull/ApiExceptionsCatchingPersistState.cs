using System;
using System.Reactive.Linq;
using Toggl.Multivac;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States.Pull
{
    internal sealed class ApiExceptionsCatchingPersistState<T> : ISyncState<IFetchObservables>
        where T : IPersistState
    {
        public T UnsafeState { get; }

        public StateResult<Exception> Failed { get; } = new StateResult<Exception>();

        public ApiExceptionsCatchingPersistState(T unsafeState)
        {
            Ensure.Argument.IsNotNull(unsafeState, nameof(unsafeState));

            UnsafeState = unsafeState;
        }

        public IObservable<ITransition> Start(IFetchObservables fetch)
            => UnsafeState.Start(fetch)
                .Catch((Exception exception) => processError(exception));

        private IObservable<ITransition> processError(Exception exception)
            => shouldRethrow(exception)
                ? Observable.Throw<ITransition>(exception)
                : Observable.Return(Failed.Transition(exception));

        private bool shouldRethrow(Exception e)
            => !(e is ServerErrorException || e is ClientErrorException)
               || e is ApiDeprecatedException
               || e is ClientDeprecatedException
               || e is UnauthorizedException;
    }
}
