using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Sync.States.Pull
{
    public sealed class SinceDateUpdatingPersistState<TInterface, TDatabaseInterface> : IPersistState
        where TInterface : ILastChangedDatable
        where TDatabaseInterface : TInterface, IDatabaseSyncable
    {
        private readonly ISinceParameterRepository sinceParameterRepository;

        private readonly IPersistState internalState;

        public StateResult<IFetchObservables> FinishedPersisting { get; } = new StateResult<IFetchObservables>();

        public SinceDateUpdatingPersistState(
            ISinceParameterRepository sinceParameterRepository,
            IPersistState internalState)
        {
            Ensure.Argument.IsNotNull(sinceParameterRepository, nameof(sinceParameterRepository));
            Ensure.Argument.IsNotNull(internalState, nameof(internalState));

            this.sinceParameterRepository = sinceParameterRepository;
            this.internalState = internalState;
        }

        public IObservable<ITransition> Start(IFetchObservables fetch)
            => internalState.Start(fetch)
                .SelectMany(_ => fetch.GetList<TInterface>()
                    .Do(maybeUpdateSinceDates)
                    .Select(__ => FinishedPersisting.Transition(fetch)));

        private void maybeUpdateSinceDates(List<TInterface> entities)
        {
            var since = entities.Max(entity => entity?.At);
            if (since.HasValue)
            {
                sinceParameterRepository.Set<TDatabaseInterface>(since);
            }
        }
    }
}
