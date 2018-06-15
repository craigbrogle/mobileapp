using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Toggl.Ultrawave;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class CheckServerStatusState : ISyncState
    {
        public StateResult Retry { get; } = new StateResult();
        public StateResult ServerIsAvailable { get; } = new StateResult();

        private ITogglApi api;
        private IScheduler scheduler;
        private IRetryDelayService apiDelay;
        private IRetryDelayService statusDelay;
        private IObservable<Unit> delayCancellation;

        public CheckServerStatusState(ITogglApi api, IScheduler scheduler, IRetryDelayService apiDelay, IRetryDelayService statusDelay, IObservable<Unit> delayCancellation)
        {
            this.api = api;
            this.scheduler = scheduler;
            this.apiDelay = apiDelay;
            this.statusDelay = statusDelay;
            this.delayCancellation = delayCancellation;
        }

        public IObservable<ITransition> Start()
            => delay(api.Status.IsAvailable())
                .Do(_ => statusDelay.Reset())
                .SelectMany(proceed)
                .Catch((Exception e) => delayedRetry(getDelay(e)));

        private IObservable<ITransition> proceed
            => Observable.Return(ServerIsAvailable.Transition());

        private IObservable<ITransition> delayedRetry(TimeSpan period)
            => Observable.Return(Unit.Default)
                .Delay(period, scheduler)
                .Merge(delayCancellation)
                .Select(_ => Retry.Transition())
                .FirstAsync();

        private IObservable<Unit> delay(IObservable<Unit> observable)
            => observable
                .Delay(apiDelay.NextSlowDelay(), scheduler)
                .Merge(delayCancellation)
                .FirstAsync();

        private TimeSpan getDelay(Exception exception)
            => exception is InternalServerErrorException
                ? statusDelay.NextSlowDelay()
                : statusDelay.NextFastDelay();
    }
}
