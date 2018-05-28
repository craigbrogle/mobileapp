using System;
using System.Reactive;
using System.Reactive.Linq;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Sync.States.Push;
using Toggl.Foundation.Tests.Sync.States.Push.BaseStates;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave.ApiClients.Interfaces;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States.Push
{
    public sealed class DeleteEntityStateTests : BasePushEntityStateTests
    {
        private readonly IDeletingApiClient<ITestModel> api
            = Substitute.For<IDeletingApiClient<ITestModel>>();

        private readonly IDataSource<IThreadSafeTestModel, IDatabaseTestModel> dataSource
            = Substitute.For<IDataSource<IThreadSafeTestModel, IDatabaseTestModel>>();

        [Fact, LogIfTooSlow]
        public void ReturnsSuccessfulTransitionWhenEverythingWorks()
        {
            var state = (DeleteEntityState<ITestModel, IDatabaseTestModel, IThreadSafeTestModel>)CreateState();
            var dirtyEntity = new TestModel(-1, SyncStatus.SyncNeeded);
            api.Delete(Arg.Any<ITestModel>())
                .Returns(Observable.Return(Unit.Default));
            dataSource.Delete(Arg.Any<long>())
                .Returns(Observable.Return(Unit.Default));

            var transition = state.Start(dirtyEntity).SingleAsync().Wait();

            transition.Result.Should().Be(state.DeletingFinished);
        }

        [Fact, LogIfTooSlow]
        public void CallsDatabaseDeleteOperationWithCorrectParameter()
        {
            var state = CreateState();
            var dirtyEntity = new TestModel(-1, SyncStatus.SyncNeeded);
            api.Delete(dirtyEntity)
                .Returns(Observable.Return(Unit.Default));

            state.Start(dirtyEntity).SingleAsync().Wait();

            dataSource.Received().Delete(dirtyEntity.Id);
        }

        [Fact, LogIfTooSlow]
        public void DoesNotDeleteTheEntityLocallyIfTheApiOperationFails()
        {
            var state = CreateState();
            var dirtyEntity = new TestModel(-1, SyncStatus.SyncNeeded);
            api.Delete(Arg.Any<ITestModel>())
                .Returns(_ => Observable.Throw<Unit>(new TestException()));

            state.Start(dirtyEntity).SingleAsync().Wait();

            dataSource.DidNotReceive().Delete(Arg.Any<long>());
        }

        [Fact, LogIfTooSlow]
        public void MakesApiCallWithCorrectParameter()
        {
            var state = CreateState();
            var dirtyEntity = new TestModel(-1, SyncStatus.SyncNeeded);
            var calledDelete = false;
            api.Delete(Arg.Is(dirtyEntity))
                .Returns(_ =>
                {
                    calledDelete = true;
                    return Observable.Return(Unit.Default);
                });

            state.Start(dirtyEntity).SingleAsync().Wait();

            calledDelete.Should().BeTrue();
        }

        protected override BasePushEntityState<IDatabaseTestModel, IThreadSafeTestModel> CreateState()
            => new DeleteEntityState<ITestModel, IDatabaseTestModel, IThreadSafeTestModel>(api, dataSource);

        protected override void PrepareApiCallFunctionToThrow(Exception e)
        {
            api.Delete(Arg.Any<ITestModel>())
                .Returns(_ => Observable.Throw<Unit>(e));
        }

        protected override void PrepareDatabaseOperationToThrow(Exception e)
        {
            dataSource.Delete(Arg.Any<long>())
                .Returns(_ => Observable.Throw<Unit>(new TestException()));
        }
    }
}
