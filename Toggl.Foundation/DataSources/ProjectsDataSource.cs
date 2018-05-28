using System;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public sealed class ProjectsDataSource
        : DataSource<IThreadSafeProject, IDatabaseProject>, IProjectsSource
    {
        private readonly IIdProvider idProvider;
        private readonly ITimeService timeService;

        public ProjectsDataSource(IIdProvider idProvider, IRepository<IDatabaseProject> repository, ITimeService timeService)
            : base(repository)
        {
            Ensure.Argument.IsNotNull(idProvider, nameof(idProvider));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));

            this.idProvider = idProvider;
            this.timeService = timeService;
        }

        public IObservable<IDatabaseProject> Create(CreateProjectDTO dto)
            => idProvider.GetNextIdentifier()
                .Apply(Project.Builder.Create)
                .SetName(dto.Name)
                .SetColor(dto.Color)
                .SetClientId(dto.ClientId)
                .SetBillable(dto.Billable)
                .SetWorkspaceId(dto.WorkspaceId)
                .SetAt(timeService.CurrentDateTime)
                .SetSyncStatus(SyncStatus.SyncNeeded)
                .Build()
                .Apply(Create);

        protected override IThreadSafeProject Convert(IDatabaseProject entity)
            => Project.From(entity);

        protected override ConflictResolutionMode ResolveConflicts(IDatabaseProject first, IDatabaseProject second)
            => Resolver.ForProjects.Resolve(first, second);
    }
}
