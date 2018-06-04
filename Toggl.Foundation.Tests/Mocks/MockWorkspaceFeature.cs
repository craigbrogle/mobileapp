using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;

namespace Toggl.Foundation.Tests.Mocks
{
    public sealed class MockWorkspaceFeature : IThreadSafeWorkspaceFeature
    {
        public WorkspaceFeatureId FeatureId { get; set; }

        public bool Enabled { get; set; }
    }
}
