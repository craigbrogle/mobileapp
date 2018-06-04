using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;

namespace Toggl.Foundation.Interactors
{
    internal sealed class GetDefaultWorkspaceInteractor : IInteractor<IObservable<IThreadSafeWorkspace>>
    {
        private readonly ITogglDataSource dataSource;

        public GetDefaultWorkspaceInteractor(ITogglDataSource dataSource)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.dataSource = dataSource;
        }

        public IObservable<IThreadSafeWorkspace> Execute()
            => dataSource.User
                .Get()
                .SelectMany(user =>
                    dataSource.Workspaces.GetById(user.DefaultWorkspaceId));
    }
}
