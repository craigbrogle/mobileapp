using System;
using Toggl.Multivac.Models;
using Newtonsoft.Json;

namespace Toggl.Ultrawave.Models
{
    internal sealed partial class Tag : ITag
    {
        private static readonly DateTimeOffset defaultAt = new DateTimeOffset(2000, 01, 02, 03, 04, 05, TimeSpan.Zero);

        public long Id { get; set; }

        public long WorkspaceId { get; set; }

        public string Name { get; set; }

        public DateTimeOffset At { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }

        [JsonConstructor]
        public Tag(long id, long workspaceId, string name, DateTimeOffset? at, DateTimeOffset? deletedAt)
        {
            Id = id;
            WorkspaceId = workspaceId;
            Name = name;
            At = at ?? defaultAt;
            DeletedAt = deletedAt;
        }
    }
}
