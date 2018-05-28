using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Toggl.Multivac.Extensions
{
    public static class ObservableExtensions
    {
        private class Observer<T> : IObserver<T>
        {
            private readonly Action<Exception> onError;
            private readonly Action onCompleted;

            public Observer(Action<Exception> onError, Action onCompleted)
            {
                this.onError = onError;
                this.onCompleted = onCompleted;
            }

            public void OnCompleted()
                => onCompleted();

            public void OnError(Exception error)
                => onError(error);

            public void OnNext(T value) { }
        }

        public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<Exception> onError, Action onCompleted)
        {
            var observer = new Observer<T>(onError, onCompleted);
            return observable.Subscribe(observer);
        }

        public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<Exception> onError)
        {
            var observer = new Observer<T>(onError, () => { });
            return observable.Subscribe(observer);
        }

        public static IObservable<T> ConnectedReplay<T>(this IObservable<T> observable)
        {
            var replayed = observable.Replay();
            replayed.Connect();
            return replayed;
        }

        public static IObservable<T> DelayIf<T>(this IObservable<T> observable, Predicate<T> predicate, TimeSpan delay)
            => observable.SelectMany(value => predicate(value)
                ? Observable.Return(value).Delay(delay)
                : Observable.Return(value));

        public static IObservable<T> RetryWhen<T, U>(this IObservable<T> source, Func<IObservable<Exception>, IObservable<U>> handler)
        {
            return Observable.Defer(() =>
            {
                var errorSignal = new Subject<Exception>();
                var retrySignal = handler(errorSignal);
                var sources = new BehaviorSubject<IObservable<T>>(source);

                return Observable.Using(
                        () => retrySignal.Select(s => source).Subscribe(sources),
                        r => sources
                            .Select(src =>
                                src.Do(v => { }, e => errorSignal.OnNext(e), () => errorSignal.OnCompleted())
                                   .OnErrorResumeNext(Observable.Empty<T>())
                            )
                            .Concat()
                    );
            });
        }
    }
}
