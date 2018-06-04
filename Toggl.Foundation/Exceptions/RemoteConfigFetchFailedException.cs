using System;
namespace Toggl.Foundation.Exceptions
{
    public sealed class RemoteConfigFetchFailedException : Exception
    {
        public RemoteConfigFetchFailedException(string message) : base(message)
        {
        }
    }
}
