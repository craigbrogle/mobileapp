using System;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public sealed class TagsDataSource
        : DataSource<IThreadSafeTag, IDatabaseTag>, ITagsSource
    {
        private readonly IIdProvider idProvider;
        private readonly ITimeService timeService;

        public TagsDataSource(IIdProvider idProvider, IRepository<IDatabaseTag> repository, ITimeService timeService)
            : base(repository)
        {
            Ensure.Argument.IsNotNull(idProvider, nameof(idProvider));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));

            this.idProvider = idProvider;
            this.timeService = timeService;
        }

        public IObservable<IThreadSafeTag> Create(string name, long workspaceId)
            => idProvider
                .GetNextIdentifier()
                .Apply(Tag.Builder.Create)
                .SetName(name)
                .SetWorkspaceId(workspaceId)
                .SetAt(timeService.CurrentDateTime)
                .SetSyncStatus(SyncStatus.SyncNeeded)
                .Build()
                .Apply(Create);

        protected override IThreadSafeTag Convert(IDatabaseTag entity)
            => Tag.From(entity);

        protected override ConflictResolutionMode ResolveConflicts(IDatabaseTag first, IDatabaseTag second)
            => Resolver.ForTags.Resolve(first, second);
    }
}
