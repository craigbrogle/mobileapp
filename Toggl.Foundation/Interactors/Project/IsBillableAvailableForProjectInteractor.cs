using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources;
using static Toggl.Multivac.WorkspaceFeatureId;

namespace Toggl.Foundation.Interactors
{
    internal sealed class IsBillableAvailableForProjectInteractor : WorkspaceHasFeatureInteractor<bool>
    {
        private readonly long projectId;

        public IsBillableAvailableForProjectInteractor(ITogglDataSource dataSource, long projectId)
            : base (dataSource)
        {
            this.projectId = projectId;
        }

        public override IObservable<bool> Execute()
            => DataSource.Projects.GetById(projectId)
                .SelectMany(project => CheckIfFeatureIsEnabled(project.WorkspaceId, Pro));
    }
}
