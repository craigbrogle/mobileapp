using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States.Push;
using Toggl.PrimeRadiant;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States.Push
{
    public sealed class PushSingleStateTests
    {
        private readonly ISingletonDataSource<IThreadSafeTestModel> dataSource
            = Substitute.For<ISingletonDataSource<IThreadSafeTestModel>>();

        [Fact, LogIfTooSlow]
        public void ConstructorThrowsWithNullDataSource()
        {
            Action creatingWithNullArgument = () => new PushSingleState<IThreadSafeTestModel>(null);

            creatingWithNullArgument.Should().Throw<ArgumentNullException>();
        }

        [Theory, LogIfTooSlow]
        [InlineData(SyncStatus.InSync)]
        [InlineData(SyncStatus.SyncFailed)]
        public void ReturnsNothingToPushTransitionWhenTheSingleEntityDoesNotNeedSyncing(SyncStatus syncStatus)
        {
            var entity = new TestModel(1, syncStatus);
            var state = new PushSingleState<IThreadSafeTestModel>(dataSource);
            dataSource.Get().Returns(Observable.Return(entity));

            var transition = state.Start().SingleAsync().Wait();

            transition.Result.Should().Be(state.NothingToPush);
        }

        [Fact, LogIfTooSlow]
        public void ReturnsPushEntityTransitionWhenTheRepositoryReturnsSomeEntity()
        {
            var state = new PushSingleState<IThreadSafeTestModel>(dataSource);
            var entity = new TestModel(1, SyncStatus.SyncNeeded);
            dataSource.Get().Returns(Observable.Return(entity));

            var transition = state.Start().SingleAsync().Wait();
            var parameter = ((Transition<IThreadSafeTestModel>)transition).Parameter;

            transition.Result.Should().Be(state.PushEntity);
            parameter.Should().BeEquivalentTo(entity, options => options.IncludingProperties());
        }

        [Fact, LogIfTooSlow]
        public void ThrowsWhenRepositoryThrows()
        {
            var state = new PushSingleState<IThreadSafeTestModel>(dataSource);
            dataSource.Get().Returns(Observable.Throw<IThreadSafeTestModel>(new Exception()));

            Action callingStart = () => state.Start().SingleAsync().Wait();

            callingStart.Should().Throw<Exception>();
        }
    }
}
