using System;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;

namespace Toggl.Foundation.Interactors
{
    internal sealed class GetWorkspaceByIdInteractor : IInteractor<IObservable<IThreadSafeWorkspace>>
    {
        private readonly long workspaceId;
        private readonly ITogglDataSource dataSource;

        public GetWorkspaceByIdInteractor(ITogglDataSource dataSource, long workspaceId)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.dataSource = dataSource;
            this.workspaceId = workspaceId;
        }

        public IObservable<IThreadSafeWorkspace> Execute()
            => dataSource.Workspaces.GetById(workspaceId);
    }
}
