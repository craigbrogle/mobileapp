using System;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DTOs
{
    public class ClientDTO : IThreadSafeClient
    {
        public long Id { get; }
        public DateTimeOffset? ServerDeletedAt { get; }
        public DateTimeOffset At { get; }
        public long WorkspaceId { get; }
        public string Name { get; }
        public SyncStatus SyncStatus { get; }
        public string LastSyncErrorMessage { get; }
        public bool IsDeleted { get; }

        public IThreadSafeWorkspace Workspace { get; }
        IDatabaseWorkspace IDatabaseClient.Workspace => Workspace;

        public ClientDTO(long id, long workspaceId, string name, DateTimeOffset at, SyncStatus syncStatus)
        {
            Id = id;
            WorkspaceId = workspaceId;
            Name = name;
            At = at;
            SyncStatus = syncStatus;
        }
    }
}
