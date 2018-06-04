using System;
using System.Reactive.Linq;
using Toggl.Foundation.Sync.States.Pull;
using Toggl.Multivac;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class ApiExceptionsCatchingPersistState : IPersistState
    {
        private readonly IPersistState internalState;

        public StateResult<IFetchObservables> FinishedPersisting { get; } = new StateResult<IFetchObservables>();

        public StateResult<Exception> Failed { get; } = new StateResult<Exception>();

        public ApiExceptionsCatchingPersistState(IPersistState internalState)
        {
            Ensure.Argument.IsNotNull(internalState, nameof(internalState));

            this.internalState = internalState;
        }

        public IObservable<ITransition> Start(IFetchObservables fetch)
            => internalState.Start(fetch)
                .Select(_ => FinishedPersisting.Transition(fetch))
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
