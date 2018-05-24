using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Parameters
{
    public sealed class CredentialsParameter
    {
        public Email Email { get; set; }

        public Password Password { get; set; }

        public static CredentialsParameter With(Email email, Password password)
            => new CredentialsParameter { Email = email, Password = password };
    }
}
