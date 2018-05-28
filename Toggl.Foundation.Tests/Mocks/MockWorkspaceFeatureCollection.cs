using System.Collections.Generic;
using System.Linq;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Tests.Mocks
{
    public sealed class MockWorkspaceFeatureCollection : IThreadSafeWorkspaceFeatureCollection
    {
        public long Id => WorkspaceId;

        public IDatabaseWorkspace Workspace { get; set; }

        public IEnumerable<IDatabaseWorkspaceFeature> DatabaseFeatures { get; set; }

        public long WorkspaceId { get; set; }

        public IEnumerable<IWorkspaceFeature> Features { get; set; }

        public bool IsEnabled(WorkspaceFeatureId feature)
            => Features.Any(f => f.FeatureId == feature && f.Enabled);
    }
}
