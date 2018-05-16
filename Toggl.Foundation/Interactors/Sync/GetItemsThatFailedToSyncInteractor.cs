using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;
using Toggl.Multivac;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Interactors
{
    public class GetItemsThatFailedToSyncInteractor<T> : IInteractor<IObservable<IEnumerable<T>>> where T : IDatabaseSyncable
    {
        private readonly IRepository<T> repository;
        private readonly Func<T, T> convert;

        public GetItemsThatFailedToSyncInteractor(IRepository<T> repository, Func<T, T> convert)
        {
            Ensure.Argument.IsNotNull(repository, nameof(repository));
            Ensure.Argument.IsNotNull(convert, nameof(convert));

            this.repository = repository;
            this.convert = convert;
        }

        public IObservable<IEnumerable<T>> Execute()
            => repository
                .GetAll(p => p.SyncStatus == SyncStatus.SyncFailed)
                .Select(items => items.Select(convert));
    }
}
