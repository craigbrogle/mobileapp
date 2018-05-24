using System;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Parameters
{
    public sealed class EmailParameter
    {
        public Email Email { get; set; }

        public static EmailParameter With(Email email)
            => new EmailParameter { Email = email };
    }
}
