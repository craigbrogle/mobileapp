using System;
using Newtonsoft.Json;
using Toggl.Multivac.Models;

namespace Toggl.Ultrawave.Models
{
    internal sealed partial class Tag : ITag
    {
        private static readonly DateTimeOffset defaultAt = new DateTimeOffset(2000, 01, 02, 03, 04, 05, TimeSpan.Zero);

        public long Id { get; set; }

        public long WorkspaceId { get; set; }

        public string Name { get; set; }

        public DateTimeOffset At { get; set; }

        [JsonProperty("deleted_at")]
        public DateTimeOffset? ServerDeletedAt { get; set; }

        [JsonConstructor]
        public Tag(long id, long workspaceId, string name, DateTimeOffset? at, DateTimeOffset? deletedAt)
        {
            Id = id;
            WorkspaceId = workspaceId;
            Name = name;
            At = at ?? defaultAt;
            ServerDeletedAt = deletedAt;
        }
    }
}
