using System;
using System.Collections.Generic;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;

namespace Toggl.Foundation.Interactors
{
    internal sealed class GetAllWorkspacesInteractor : IInteractor<IObservable<IEnumerable<IThreadSafeWorkspace>>>
    {
        private readonly ITogglDataSource dataSource;

        public GetAllWorkspacesInteractor(ITogglDataSource dataSource)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.dataSource = dataSource;
        }

        public IObservable<IEnumerable<IThreadSafeWorkspace>> Execute()
            => dataSource.Workspaces.GetAll();
    }
}
