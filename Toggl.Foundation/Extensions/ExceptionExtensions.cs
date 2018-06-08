using System;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Extensions
{
    public static class ExceptionExtensions
    {
        public static bool CanContainSensitiveInformation(this Exception exception)
            => !isWhiteListed(exception);

        private static bool isWhiteListed(Exception exception)
            => exception is ApiException;
    }
}
