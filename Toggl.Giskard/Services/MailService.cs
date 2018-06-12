using System;
using System.Threading.Tasks;
using Android.Content;
using Toggl.Foundation.Services;
using Uri = Android.Net.Uri;

namespace Toggl.Giskard.Services
{
    public sealed class MailService : IMailService
    {
        private readonly Context context;

        public MailService(Context context)
        {
            this.context = context;
        }

        public Task<MailResult> Send(string recipient, string subject, string message)
        {
            try
            {
                var emailIntent = new Intent(Intent.ActionSendto);

                emailIntent.PutExtra(Intent.ExtraEmail, new String[] { recipient });
                emailIntent.PutExtra(Intent.ExtraSubject, subject);
                emailIntent.PutExtra(Intent.ExtraText, message);
                emailIntent.SetData(Uri.Parse("mailto:"));

                var chooserText = context.Resources.GetString(Resource.String.FeedbackChooserCopy);
                var chooserIntent = Intent.CreateChooser(emailIntent, chooserText);
                chooserIntent.AddFlags(ActivityFlags.NewTask);

                if (chooserIntent.ResolveActivity(context.PackageManager) == null)
                    throw new ActivityNotFoundException();

                context.StartActivity(chooserIntent);
                return Task.FromResult(MailResult.Ok);
            }
            catch (ActivityNotFoundException)
            {
            }

            return Task.FromResult(getNoEmailHandlerWarningResult());
        }

        private MailResult getNoEmailHandlerWarningResult()
        {
            var title = context.Resources.GetString(Resource.String.NoEmailHandlerErrorTitle);
            var message = context.Resources.GetString(Resource.String.NoEmailHandlerErrorMessage);

            return new MailResult(false, title, message);
        }
    }
}
