using System;

namespace Toggl.Foundation.Interactors
{
    public sealed partial class InteractorFactory : IInteractorFactory
    {
        public IInteractor<IObservable<bool>> IsBillableAvailableForProject(long projectId)
            => new IsBillableAvailableForProjectInteractor(dataSource, projectId);

        public IInteractor<IObservable<bool>> ProjectDefaultsToBillable(long projectId)
            => new ProjectDefaultsToBillableInteractor(dataSource, projectId);
    }
}
