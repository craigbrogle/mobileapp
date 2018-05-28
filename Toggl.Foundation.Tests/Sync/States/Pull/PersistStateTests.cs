using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Tests.Helpers;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave.Exceptions;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class PersistStateTests
    {
        private readonly IBaseDataSource<IThreadSafeTestModel, IDatabaseTestModel> dataSource;
        private readonly ISinceParameterRepository sinceParameterRepository;
        private readonly PersistState<ITestModel, IDatabaseTestModel, IThreadSafeTestModel> state;

        private DateTimeOffset now { get; } = new DateTimeOffset(2017, 04, 05, 12, 34, 56, TimeSpan.Zero);
        private DateTimeOffset at = new DateTimeOffset(2017, 09, 01, 12, 34, 56, TimeSpan.Zero);

        public PersistStateTests()
        {
            dataSource = Substitute.For<IBaseDataSource<IThreadSafeTestModel, IDatabaseTestModel>>();
            sinceParameterRepository = Substitute.For<ISinceParameterRepository>();
            state = new PersistState<ITestModel, IDatabaseTestModel, IThreadSafeTestModel>(dataSource, sinceParameterRepository, TestModel.Clean);
        }

        [Fact, LogIfTooSlow]
        public void EmitsTransitionToPersistFinished()
        {
            var observables = createObservables();

            var transition = state.Start(observables).SingleAsync().Wait();

            transition.Result.Should().Be(state.FinishedPersisting);
        }

        [Fact, LogIfTooSlow]
        public void ThrowsIfFetchObservablePublishesTwice()
        {
            var fetchObservables = createFetchObservables(
                null,
                Observable.Create<List<ITestModel>>(observer =>
                {
                    observer.OnNext(new List<ITestModel>());
                    observer.OnNext(new List<ITestModel>());
                    return () => { };
                }));

            Action fetchTwice = () => state.Start(fetchObservables).Wait();

            fetchTwice.ShouldThrow<InvalidOperationException>();
        }

        [Fact, LogIfTooSlow]
        public void TriggersBatchUpdate()
        {
            var entities = createComplexListWhereTheLastUpdateEntityIsDeleted(at);
            var observables = createObservables(entities);

            state.Start(observables).SingleAsync().Wait();

            assertBatchUpdateWasCalled(entities);
        }

        [Fact, LogIfTooSlow]
        public void DoesNotUpdateSinceParametersWhenNothingIsFetched()
        {
            var observables = createObservables();

            state.Start(observables).SingleAsync().Wait();

            sinceParameterRepository.DidNotReceive().Set<IDatabaseTestModel>(Arg.Any<DateTimeOffset?>());
        }

        [Fact, LogIfTooSlow]
        public void UpdatesSinceParametersOfTheFetchedEntity()
        {
            var newAt = new DateTimeOffset(2017, 10, 01, 12, 34, 56, TimeSpan.Zero);
            var entities = new List<ITestModel> { new TestModel { At = newAt } };
            var observables = createObservables(entities);
            setupDatabaseBatchUpdateMocksToReturnUpdatedDatabaseEntitiesAndSimulateDeletionOfEntities(entities);
            sinceParameterRepository.Supports<IDatabaseTestModel>().Returns(true);

            state.Start(observables).SingleAsync().Wait();

            sinceParameterRepository.Received().Set<IDatabaseTestModel>(Arg.Is(newAt));
        }

        [Fact, LogIfTooSlow]
        public void HandlesNullValueReceivedFromTheServerAsAnEmptyList()
        {
            List<ITestModel> entities = null;
            var observables = createObservables(entities);

            var transition = (Transition<IFetchObservables>)state.Start(observables).SingleAsync().Wait();

            transition.Result.Should().Be(state.FinishedPersisting);
            assertBatchUpdateWasCalled(new List<ITestModel>());
        }

        [Fact, LogIfTooSlow]
        public void SelectsTheLatestAtValue()
        {
            var entities = createComplexListWhereTheLastUpdateEntityIsDeleted(at);
            var observables = createObservables(entities);
            setupDatabaseBatchUpdateMocksToReturnUpdatedDatabaseEntitiesAndSimulateDeletionOfEntities(entities);
            sinceParameterRepository.Supports<IDatabaseTestModel>().Returns(true);

            state.Start(observables).SingleAsync().Wait();

            sinceParameterRepository.Received().Set<IDatabaseTestModel>(Arg.Is<DateTimeOffset?>(at));
        }   

        [Fact, LogIfTooSlow]
        public void SinceDatesAreNotUpdatedWhenBatchUpdateThrows()
        {
            var entities = createComplexListWhereTheLastUpdateEntityIsDeleted(at);
            var observables = createObservables(entities);
            setupBatchUpdateToThrow(new TestException());

            try
            {
                state.Start(observables).SingleAsync().Wait();
            }
            catch (TestException) { }

            sinceParameterRepository.DidNotReceiveWithAnyArgs().Set<IDatabaseTestModel>(null);
        }

        [Fact, LogIfTooSlow]
        public void ThrowsWhenBatchUpdateThrows()
        {
            var observables = createObservables();
            setupBatchUpdateToThrow(new TestException());

            Action startingState = () => state.Start(observables).SingleAsync().Wait();

            startingState.ShouldThrow<TestException>();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ClientExceptionsWhichAreNotReThrownInSyncStates), MemberType = typeof(ApiExceptions))]
        public void ReturnsClientErrorTransitionWhenHttpFailsWithClientErrorException(ClientErrorException exception)
        {
            var state = new PersistState<ITestModel, IDatabaseTestModel, IThreadSafeTestModel>(dataSource, sinceParameterRepository, TestModel.Clean);
            var observables = createFetchObservablesWhichThrow(exception);

            var transition = state.Start(observables).SingleAsync().Wait();
            var reason = ((Transition<Exception>)transition).Parameter;

            transition.Result.Should().Be(state.Failed);
            reason.Should().BeAssignableTo<ClientErrorException>();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ServerExceptions), MemberType = typeof(ApiExceptions))]
        public void ReturnsServerErrorTransitionWhenHttpFailsWithServerErrorException(ServerErrorException exception)
        {
            var state = new PersistState<ITestModel, IDatabaseTestModel, IThreadSafeTestModel>(dataSource, sinceParameterRepository, TestModel.Clean);
            var observables = createFetchObservablesWhichThrow(exception);

            var transition = state.Start(observables).SingleAsync().Wait();
            var reason = ((Transition<Exception>)transition).Parameter;

            transition.Result.Should().Be(state.Failed);
            reason.Should().BeAssignableTo<ServerErrorException>();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ExceptionsWhichCauseRethrow), MemberType = typeof(ApiExceptions))]
        public void ThrowsWhenExceptionsWhichShouldCauseRethrowAreCaught(Exception exception)
        {
            var state = new PersistState<ITestModel, IDatabaseTestModel, IThreadSafeTestModel>(dataSource, sinceParameterRepository, TestModel.Clean);
            Exception caughtException = null;
            var observables = createFetchObservablesWhichThrow(exception);

            try
            {
                state.Start(observables).Wait();
            }
            catch (Exception e)
            {
                caughtException = e;
            }

            caughtException.Should().NotBeNull();
            caughtException.Should().BeAssignableTo(exception.GetType());
        }

        private IFetchObservables createFetchObservables(IFetchObservables old, IObservable<List<ITestModel>> observable = null) 
        {
            var observables = Substitute.For<IFetchObservables>();
            observables.Get<ITestModel>().Returns(observable ?? old?.Get<ITestModel>());
            return observables;
        }
            

        private List<ITestModel> createComplexListWhereTheLastUpdateEntityIsDeleted(DateTimeOffset? maybeAt)
        {
            var at = maybeAt ?? now;
            return new List<ITestModel>
            {
                new TestModel { At = at.AddDays(-1), Id = 0 },
                new TestModel { At = at.AddDays(-3), Id = 1 },
                new TestModel { At = at, Id = 2, ServerDeletedAt = at.AddDays(-1) },
                new TestModel { At = at.AddDays(-2), Id = 3 }
            };
        }

        private IFetchObservables createObservables(List<ITestModel> entities = null)
            => createFetchObservables(null, Observable.Return(entities));

        private void setupDatabaseBatchUpdateMocksToReturnUpdatedDatabaseEntitiesAndSimulateDeletionOfEntities(List<ITestModel> entities = null)
        {
            var foundationEntities = entities?.Select(entity => entity.ServerDeletedAt.HasValue
                ? (IConflictResolutionResult<IThreadSafeTestModel>)new DeleteResult<IThreadSafeTestModel>(0)
                : new UpdateResult<IThreadSafeTestModel>(0, TestModel.Clean(entity)));
            dataSource.BatchUpdate(null)
                .ReturnsForAnyArgs(Observable.Return(foundationEntities));
        }

        private void setupBatchUpdateToThrow(Exception exception)
            => dataSource.BatchUpdate(null).ReturnsForAnyArgs(_ => throw exception);

        private void assertBatchUpdateWasCalled(List<ITestModel> entities = null)
        {
            dataSource.Received().BatchUpdate(Arg.Is<IEnumerable<IThreadSafeTestModel>>(
                list => list.Count() == entities.Count && list.All(
                    persisted => persisted.SyncStatus == SyncStatus.InSync && entities.Any(te => te.Id == persisted.Id))));
        }

        private IFetchObservables createFetchObservablesWhichThrow(Exception exception)
            => createFetchObservables(null, Observable.Throw<List<ITestModel>>(exception));
    }
}
