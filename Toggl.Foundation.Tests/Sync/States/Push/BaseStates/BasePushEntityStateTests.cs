using System;
using System.Reactive.Linq;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States.Push;
using Toggl.Foundation.Tests.Helpers;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave.Exceptions;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States.Push.BaseStates
{
    public abstract class BasePushEntityStateTests
    {
        [Fact, LogIfTooSlow]
        public void ReturnsFailTransitionWhenEntityIsNull()
        {
            var state = CreateState();

            var transition = state.Start(null).SingleAsync().Wait();
            var parameter = ((Transition<(Exception Reason, IThreadSafeTestModel)>)transition).Parameter;

            transition.Result.Should().Be(state.UnknownError);
            parameter.Reason.Should().BeOfType<ArgumentNullException>();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ClientExceptionsWhichAreNotReThrownInSyncStates), MemberType = typeof(ApiExceptions))]
        public void ReturnsClientErrorTransitionWhenHttpFailsWithClientErrorException(ClientErrorException exception)
        {
            var state = CreateState();
            var entity = new TestModel(-1, SyncStatus.SyncNeeded);
            PrepareApiCallFunctionToThrow(exception);

            var transition = state.Start(entity).SingleAsync().Wait();
            var parameter = ((Transition<(Exception Reason, IThreadSafeTestModel)>)transition).Parameter;

            transition.Result.Should().Be(state.ClientError);
            parameter.Reason.Should().BeAssignableTo<ClientErrorException>();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ServerExceptions), MemberType = typeof(ApiExceptions))]
        public void ReturnsServerErrorTransitionWhenHttpFailsWithServerErrorException(ServerErrorException exception)
        {
            var state = CreateState();
            var entity = new TestModel(-1, SyncStatus.SyncNeeded);
            PrepareApiCallFunctionToThrow(exception);

            var transition = state.Start(entity).SingleAsync().Wait();
            var parameter = ((Transition<(Exception Reason, IThreadSafeTestModel)>)transition).Parameter;

            transition.Result.Should().Be(state.ServerError);
            parameter.Reason.Should().BeAssignableTo<ServerErrorException>();
        }

        [Fact, LogIfTooSlow]
        public void ReturnsUnknownErrorTransitionWhenHttpFailsWithNonApiException()
        {
            var state = CreateState();
            var entity = new TestModel(-1, SyncStatus.SyncNeeded);
            PrepareApiCallFunctionToThrow(new TestException());

            var transition = state.Start(entity).SingleAsync().Wait();
            var parameter = ((Transition<(Exception Reason, IThreadSafeTestModel)>)transition).Parameter;

            transition.Result.Should().Be(state.UnknownError);
            parameter.Reason.Should().BeOfType<TestException>();
        }

        [Fact, LogIfTooSlow]
        public void ReturnsFailTransitionWhenDatabaseOperationFails()
        {
            var state = CreateState();
            var entity = new TestModel(-1, SyncStatus.SyncNeeded);
            PrepareDatabaseOperationToThrow(new TestException());

            var transition = state.Start(entity).SingleAsync().Wait();
            var parameter = ((Transition<(Exception Reason, IThreadSafeTestModel)>)transition).Parameter;

            transition.Result.Should().Be(state.UnknownError);
            parameter.Reason.Should().BeOfType<TestException>();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ExceptionsWhichCauseRethrow), MemberType = typeof(ApiExceptions))]
        public void ThrowsWhenExceptionsWhichShouldBeRethrownAreCaught(Exception exception)
        {
            var state = CreateState();
            PrepareApiCallFunctionToThrow(exception);
            Exception caughtException = null;

            try
            {
                state.Start(Substitute.For<IThreadSafeTestModel>()).Wait();
            }
            catch (Exception e)
            {
                caughtException = e;
            }

            caughtException.Should().NotBeNull();
            caughtException.Should().BeAssignableTo(exception.GetType());
        }

        protected abstract BasePushEntityState<IThreadSafeTestModel> CreateState();

        protected abstract void PrepareApiCallFunctionToThrow(Exception e);

        protected abstract void PrepareDatabaseOperationToThrow(Exception e);
    }
}
