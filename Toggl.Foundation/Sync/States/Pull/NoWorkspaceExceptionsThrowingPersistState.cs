using System;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.Sync.States.Pull;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class NoWorkspaceExceptionsThrowingPersistState : IPersistState
    {
        private readonly IPersistState internalState;

        public StateResult<IFetchObservables> FinishedPersisting { get; } = new StateResult<IFetchObservables>();

        public NoWorkspaceExceptionsThrowingPersistState(IPersistState internalState)
        {
            Ensure.Argument.IsNotNull(internalState, nameof(internalState));

            this.internalState = internalState;
        }

        public IObservable<ITransition> Start(IFetchObservables fetch)
            => fetch.GetList<IWorkspace>()
                .SelectMany(workspaces => workspaces.Any()
                    ? handlePresenceOfWorkspaces(fetch)
                    : Observable.Throw<ITransition>(new NoWorkspaceException()));

        private IObservable<ITransition> handlePresenceOfWorkspaces(IFetchObservables fetch)
            => internalState.Start(fetch)
                .Select(FinishedPersisting.Transition(fetch));
    }
}
