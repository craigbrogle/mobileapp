using System;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Android.Graphics;

namespace Toggl.Giskard.Helper
{
    public static class ImageUtils
    {
        public static IObservable<Bitmap> GetImageFromUrl(string url)
        {
            return Observable.Create<Bitmap>(async observer =>
            {
                try
                {
                    using (var webClient = new WebClient())
                    {
                        var imageBytes = await webClient.DownloadDataTaskAsync(new Uri(url));

                        if (imageBytes == null || imageBytes.Length == 0)
                            throw new InvalidOperationException("No image retrieved from the URL.");

                        var bitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                        observer.OnNext(bitmap);

                        observer.OnCompleted();
                    }
                }
                catch(Exception ex)
                {
                    observer.OnError(ex);
                }
            });
        }
    }
}
