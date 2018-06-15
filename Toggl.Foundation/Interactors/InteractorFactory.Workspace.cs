using System;
using System.Collections.Generic;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors
{
    public sealed partial class InteractorFactory : IInteractorFactory
    {
        public IInteractor<IObservable<IThreadSafeWorkspace>> GetDefaultWorkspace()
            => new GetDefaultWorkspaceInteractor(dataSource);

        public IInteractor<IObservable<bool>> AreCustomColorsEnabledForWorkspace(long workspaceId)
            => new AreCustomColorsEnabledForWorkspaceInteractor(dataSource, workspaceId);

        public IInteractor<IObservable<bool?>> AreProjectsBillableByDefault(long workspaceId)
            => new AreProjectsBillableByDefaultInteractor(dataSource, workspaceId);

        public IInteractor<IObservable<IThreadSafeWorkspace>> GetWorkspaceById(long workspaceId)
            => new GetWorkspaceByIdInteractor(dataSource, workspaceId);

        public IInteractor<IObservable<IEnumerable<IThreadSafeWorkspace>>> GetAllWorkspaces()
            => new GetAllWorkspacesInteractor(dataSource);

        public IInteractor<IObservable<bool>> WorkspaceAllowsBillableRates(long workspaceId)
            => new WorkspaceAllowsBillableRatesInteractor(dataSource, workspaceId);

        public IInteractor<IObservable<bool>> IsBillableAvailableForWorkspace(long workspaceId)
            => new IsBillableAvailableForWorkspaceInteractor(dataSource, workspaceId);
    }
}
