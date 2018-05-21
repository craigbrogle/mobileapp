using System;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Parameters
{
    [Preserve(AllMembers = true)]
    public sealed class SelectTimeResultsParameters
    {
        public DateTimeOffset Start { get; }
        public DateTimeOffset? Stop { get; }

        public SelectTimeResultsParameters(DateTimeOffset start, DateTimeOffset? stop)
        {
            Start = start;
            Stop = stop;
        }
    }
}
