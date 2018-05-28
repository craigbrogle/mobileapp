using System;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors
{
    internal sealed class DeleteTimeEntryInteractor : IInteractor<IObservable<Unit>>
    {
        private readonly ITimeEntriesSource dataSource;
        private readonly long id;

        public DeleteTimeEntryInteractor(
            ITimeEntriesSource dataSource,
            long id)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.dataSource = dataSource;
            this.id = id;
        }

        public IObservable<Unit> Execute()
            => dataSource.GetById(id)
                .SelectMany(dataSource.SoftDelete);
    }
}
