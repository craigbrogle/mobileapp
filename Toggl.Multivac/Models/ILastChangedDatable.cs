using System;

namespace Toggl.Multivac.Models
{
    public interface ILastChangedDatable
    {
        DateTimeOffset At { get; }
    }
}
