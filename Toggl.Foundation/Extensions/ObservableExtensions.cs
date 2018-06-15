using System;
using System.Reactive.Linq;
using Toggl.Foundation.Analytics;

namespace Toggl.Foundation.Extensions
{
    public static class ObservableExtensions
    {
        public static IObservable<T> Track<T>(this IObservable<T> observable, IAnalyticsEvent analyticsEvent)
            => observable.Do(_ => analyticsEvent.Track());

        public static IObservable<T> Track<T>(this IObservable<T> observable, IAnalyticsEvent<T> analyticsEvent)
            => observable.Do(analyticsEvent.Track);

        public static IObservable<T> Track<T, T1>(this IObservable<T> observable, IAnalyticsEvent<T1> analyticsEvent, T1 parameter)
            => observable.Do(_ => analyticsEvent.Track(parameter));

        public static IObservable<T> Track<T, T1, T2>(this IObservable<T> observable, IAnalyticsEvent<T1, T2> analyticsEvent, T1 param1, T2 param2)
            => observable.Do(_ => analyticsEvent.Track(param1, param2));
    }
}
