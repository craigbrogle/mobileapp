using System;
using System.Reactive;
using System.Threading.Tasks;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.Extensions
{
    public static class ObservableExtensions
    {
        public static IObservable<T> AsDriver<T>(this IObservable<T> observable)
            => observable.AsDriver(default(T));

        public static IDisposable VoidSubscribe(this IObservable<Unit> observable, Action onNext)
            => observable.Subscribe((Unit _) => onNext());

        public static IDisposable Subscribe(this IObservable<Unit> observable, Func<Task> onNext)
            => observable.Subscribe(async (Unit _) => await onNext());
    }
}
