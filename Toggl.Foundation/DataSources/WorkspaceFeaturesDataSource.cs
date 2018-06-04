using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public sealed class WorkspaceFeaturesDataSource : DataSource<IThreadSafeWorkspaceFeatureCollection, IDatabaseWorkspaceFeatureCollection>
    {
        public WorkspaceFeaturesDataSource(IRepository<IDatabaseWorkspaceFeatureCollection> repository)
            : base(repository)
        {
        }

        protected override IThreadSafeWorkspaceFeatureCollection Convert(IDatabaseWorkspaceFeatureCollection entity)
            => WorkspaceFeatureCollection.From(entity);

        protected override ConflictResolutionMode ResolveConflicts(IDatabaseWorkspaceFeatureCollection first,
            IDatabaseWorkspaceFeatureCollection second)
            => Resolver.ForWorkspaceFeatures.Resolve(first, second);
    }
}
