using System;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Tests.Mocks
{
    public sealed class MockTask : IThreadSafeTask
    {
        public IDatabaseUser User { get; set; }

        public IDatabaseProject Project { get; set; }

        public IDatabaseWorkspace Workspace { get; set; }

        public string Name { get; set; }

        public long ProjectId { get; set; }

        public long WorkspaceId { get; set; }

        public long? UserId { get; set; }

        public long EstimatedSeconds { get; set; }

        public bool Active { get; set; }

        public DateTimeOffset At { get; set; }

        public long TrackedSeconds { get; set; }

        public long Id { get; set; }

        public SyncStatus SyncStatus { get; set; }

        public string LastSyncErrorMessage { get; set; }

        public bool IsDeleted { get; set; }
    }
}
