using System;
namespace Toggl.Foundation.Analytics
{
    public enum LoginErrorSource
    {
        InvalidEmailOrPassword,
        GoogleLoginError,
        Offline,
        ServerError,
        MissingApiToken,
        Other
    }
}
