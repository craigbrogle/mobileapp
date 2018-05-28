using System;

namespace Toggl.Multivac.Models
{
    public interface IDeletable
    {
        DateTimeOffset? ServerDeletedAt { get; }
    }
}
