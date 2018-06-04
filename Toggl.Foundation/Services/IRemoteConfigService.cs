using System;
using Toggl.Multivac;

namespace Toggl.Foundation.Services
{
    public class IRemoteConfigService
    {
        IObservable<RatingViewConfiguration> RatingViewConfiguration { get; }
    }
}
