using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Helper;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Exceptions;

namespace Toggl.Foundation.Sync.States.Pull
{
    public sealed class CreateGhostProjectsState : IPersistState
    {
        private readonly IProjectsSource dataSource;

        public StateResult<IFetchObservables> FinishedPersisting { get; } = new StateResult<IFetchObservables>();

        public CreateGhostProjectsState(IProjectsSource dataSource)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.dataSource = dataSource;
        }

        public IObservable<ITransition> Start(IFetchObservables fetch)
            => fetch.GetList<ITimeEntry>()
                .SingleAsync()
                .SelectMany(CommonFunctions.Identity)
                .Distinct(timeEntry => timeEntry.ProjectId)
                .SelectMany(createGhostForTimeEntryIfProjectIsNotInDatabase)
                .ToList()
                .Select(FinishedPersisting.Transition(fetch));

        private IObservable<Unit> createGhostForTimeEntryIfProjectIsNotInDatabase(ITimeEntry timeEntry)
            => timeEntry.ProjectId.HasValue
                ? dataSource.GetAll(project => project.Id == timeEntry.ProjectId.Value)
                    .Select(projects => projects.Any())
                    .SelectMany(someProjectExists => !someProjectExists
                        ? createGhostProjectFor(timeEntry.WorkspaceId, timeEntry.ProjectId.Value).SelectUnit()
                        : Observable.Return(Unit.Default))
                : Observable.Return(Unit.Default);

        private IObservable<IThreadSafeProject> createGhostProjectFor(long workspaceId, long projectId)
        {
            var ghost = Project.Builder.Create(projectId)
                .SetName(Resources.InaccessibleProject)
                .SetWorkspaceId(workspaceId)
                .SetColor(Color.NoProject)
                .SetActive(false)
                .SetAt(default(DateTimeOffset))
                .SetSyncStatus(SyncStatus.RefetchingNeeded)
                .Build();

            return dataSource.Create(ghost);
        }
    }
}
