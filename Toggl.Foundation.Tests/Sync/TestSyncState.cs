using System;
using Toggl.Foundation.Sync;

namespace Toggl.Foundation.Tests.Sync
{
    public class TestSyncState : ISyncState
    {
        private readonly Func<IObservable<ITransition>> start;

        public TestSyncState(Func<IObservable<ITransition>> start)
        {
            this.start = start;
        }

        public IObservable<ITransition> Start() => start();
    }

    public class TestSyncState<T> : ISyncState<T>
    {
        private readonly Func<T, IObservable<ITransition>> start;

        public TestSyncState(Func<T, IObservable<ITransition>> start)
        {
            this.start = start;
        }

        public IObservable<ITransition> Start(T parameter) => start(parameter);
    }
}
