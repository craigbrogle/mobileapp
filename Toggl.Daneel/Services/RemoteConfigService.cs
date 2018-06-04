using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Firebase.RemoteConfig;
using Toggl.Foundation.Services;
using Toggl.Multivac;

namespace Toggl.Daneel.Services
{
    public sealed class RemoteConfigService : IRemoteConfigService
    {
        public IObservable<RatingViewConfiguration> RatingViewConfiguration
        {
            get
            {
                var ratingViewConfigurationSubject = new Subject<RatingViewConfiguration>();
                var remoteConfig = RemoteConfig.SharedInstance;
                remoteConfig.Fetch((status, error) =>
                {
                    //TODO:
                    if (error != null)
                        ratingViewConfigurationSubject.OnError(new Exception());

                    remoteConfig.ActivateFetched();
                    var configuration = new RatingViewConfiguration(
                        remoteConfig["day_count"].NumberValue.Int32Value,
                        remoteConfig["criterion"].StringValue
                    );
                    ratingViewConfigurationSubject.OnNext(configuration);
                });
                return ratingViewConfigurationSubject.AsObservable();
            }
        }
    }
}
