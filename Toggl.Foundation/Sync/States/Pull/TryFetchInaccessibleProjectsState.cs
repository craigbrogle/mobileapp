﻿using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave.ApiClients;
using System.Collections.Generic;

namespace Toggl.Foundation.Sync.States.Pull
{
    public sealed class TryFetchInaccessibleProjectsState : IPersistState
    {
        private readonly IProjectsSource dataSource;

        private readonly ITimeService timeService;

        private readonly IProjectsApi projectsApi;

        public StateResult<IFetchObservables> FetchNext { get; } = new StateResult<IFetchObservables>();

        public StateResult<IFetchObservables> FinishedPersisting { get; } = new StateResult<IFetchObservables>();

        private DateTimeOffset yesterdayThisTime => timeService.CurrentDateTime.AddDays(-1);

        public TryFetchInaccessibleProjectsState(
            IProjectsSource dataSource,
            ITimeService timeService,
            IProjectsApi projectsApi)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(projectsApi, nameof(projectsApi));

            this.dataSource = dataSource;
            this.timeService = timeService;
            this.projectsApi = projectsApi;
        }

        public IObservable<ITransition> Start(IFetchObservables fetch)
            => getProjectsWhichNeedsRefetching()
                .SelectMany(projects => projects == null
                    ? Observable.Return(FinishedPersisting.Transition(fetch))
                    : refetch(projects).Select(FetchNext.Transition(fetch)));

        private IObservable<IGrouping<long, IThreadSafeProject>> getProjectsWhichNeedsRefetching()
            => dataSource.GetAll(project =>
                    project.SyncStatus == SyncStatus.RefetchingNeeded && project.At < yesterdayThisTime)
                .SelectMany(projects =>
                    projects.GroupBy(project => project.WorkspaceId).OrderBy(group => group.Key))
                .FirstOrDefaultAsync();

        private IObservable<Unit> refetch(IGrouping<long, IThreadSafeProject> projectsToFetch)
            => projectsApi.Search(
                    workspaceId: projectsToFetch.Key,
                    projectIds: projectsToFetch.Select(p => p.Id).ToArray())
                .SelectMany(CommonFunctions.Identity)
                .Select(Project.Clean)
                .SelectMany(dataSource.Update)
                .ToList()
                .SelectMany(projectsWhichWereNotFetched(projectsToFetch))
                .SelectMany(updateAtValue)
                .ToList()
                .SelectUnit();

        private Func<IList<IThreadSafeProject>, IEnumerable<IThreadSafeProject>> projectsWhichWereNotFetched(IEnumerable<IThreadSafeProject> searchedProjects)
            => foundProjects => searchedProjects.Where(
                searchedProject => !foundProjects.Any(foundProject => foundProject.Id == searchedProject.Id));

        private IObservable<IThreadSafeProject> updateAtValue(IThreadSafeProject project)
        {
            var updatedProject = Project.Builder.From(project)
                .SetAt(timeService.CurrentDateTime)
                .Build();

            return dataSource.Update(updatedProject);
        }
    }
}
