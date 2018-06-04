using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources;
using Toggl.Multivac;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Interactors
{
    internal sealed class ProjectDefaultsToBillableInteractor : IInteractor<IObservable<bool>>
    {
        private readonly long projectId;
        private readonly ITogglDataSource dataSource;

        public ProjectDefaultsToBillableInteractor(ITogglDataSource dataSource, long projectId)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.projectId = projectId;
            this.dataSource = dataSource;
        }

        public IObservable<bool> Execute()
            => dataSource.Projects
                .GetById(projectId)
                .Select(project => project.Billable ?? false);
    }
}
