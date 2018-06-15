using System;
using System.Net;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources;

namespace Toggl.Foundation.Interactors
{
    internal sealed class GetUserAvatarInteractor : IInteractor<IObservable<byte[]>>
    {
        private readonly string url;

        public GetUserAvatarInteractor(string url)
        {
            this.url = url;
        }

        public IObservable<byte[]> Execute() => 
            Observable.Create<byte[]>(async observer =>
            {
                try
                {
                    using (var webClient = new WebClient())
                    {
                        var imageBytes = await webClient.DownloadDataTaskAsync(new Uri(url));
                        if (imageBytes == null || imageBytes.Length == 0)
                            throw new InvalidOperationException("No image retrieved from the URL.");

                        observer.OnNext(imageBytes);
                        observer.OnCompleted();
                    }
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }
            });
    }
}
