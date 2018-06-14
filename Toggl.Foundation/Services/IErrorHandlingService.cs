using System;

namespace Toggl.Foundation.Services
{
    public interface IErrorHandlingService
    {
        bool TryHandleDeprecationError(Exception error);
        bool TryHandleUnauthorizedError(Exception error);
        bool TryHandleNoWorkspaceError(Exception error);
    }
}
