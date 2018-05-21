using System;
using Toggl.Foundation.DataSources;
using Toggl.Multivac;

namespace Toggl.Foundation.Login
{
    public interface ILoginManager
    {
        ITogglDataSource GetDataSourceIfLoggedIn();

        IObservable<ITogglDataSource> LoginWithGoogle();
        IObservable<ITogglDataSource> Login(Email email, Password password);

        IObservable<ITogglDataSource> SignUpWithGoogle();
        IObservable<ITogglDataSource> SignUp(Email email, Password password, bool termsAccepted, int? countryId);

        IObservable<ITogglDataSource> RefreshToken(Password password);

        IObservable<string> ResetPassword(Email email);
    }
}
