using System.Linq;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Models
{
    internal partial class WorkspaceFeatureCollection
    {
        public long Id => WorkspaceId;

        public static WorkspaceFeatureCollection From(IWorkspaceFeatureCollection entity)
            => new WorkspaceFeatureCollection(entity);

        public bool IsEnabled(WorkspaceFeatureId feature)
            => Features.Any(x => x.FeatureId == feature);
    }
}
