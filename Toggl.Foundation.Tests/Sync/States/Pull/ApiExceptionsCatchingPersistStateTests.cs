using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Sync.States.Pull;
using Toggl.Foundation.Tests.Helpers;
using Toggl.Ultrawave.Exceptions;
using Toggl.Ultrawave.Network;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class ApiExceptionsCatchingPersistStateTests
    {
        private readonly IFetchObservables fetchObservables = Substitute.For<IFetchObservables>();

        private readonly IPersistState internalState = Substitute.For<IPersistState>();

        private readonly ApiExceptionsCatchingPersistState<IPersistState> state;

        public ApiExceptionsCatchingPersistStateTests()
        {
            state = new ApiExceptionsCatchingPersistState<IPersistState>(internalState);
        }

        [Fact]
        public async Task ReturnsTransitionFromInternalStateWhenEverythingWorks()
        {
            var internalTransition = Substitute.For<ITransition>();
            internalState.Start(Arg.Any<IFetchObservables>()).Returns(Observable.Return(internalTransition));

            var transition = await state.Start(fetchObservables);

            transition.Should().Be(internalTransition);
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ClientExceptionsWhichAreNotReThrownInSyncStates), MemberType = typeof(ApiExceptions))]
        public void ReturnsClientErrorTransitionWhenHttpFailsWithClientErrorException(ClientErrorException exception)
        {
            internalState.Start(Arg.Any<IFetchObservables>()).Returns(Observable.Throw<ITransition>(exception));

            var transition = state.Start(fetchObservables).SingleAsync().Wait();
            var reason = ((Transition<Exception>)transition).Parameter;

            transition.Result.Should().Be(state.Failed);
            reason.Should().BeAssignableTo<ClientErrorException>();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ServerExceptions), MemberType = typeof(ApiExceptions))]
        public void ReturnsServerErrorTransitionWhenHttpFailsWithServerErrorException(ServerErrorException exception)
        {
            internalState.Start(Arg.Any<IFetchObservables>()).Returns(Observable.Throw<ITransition>(exception));

            var transition = state.Start(fetchObservables).SingleAsync().Wait();
            var reason = ((Transition<Exception>)transition).Parameter;

            transition.Result.Should().Be(state.Failed);
            reason.Should().BeAssignableTo<ServerErrorException>();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ExceptionsWhichCauseRethrow), MemberType = typeof(ApiExceptions))]
        [MemberData(nameof(ExtraExceptionsToRethrow))]
        public void ThrowsWhenExceptionsWhichShouldCauseRethrowAreCaught(Exception exception)
        {
            Exception caughtException = null;
            internalState.Start(Arg.Any<IFetchObservables>()).Returns(Observable.Throw<ITransition>(exception));

            try
            {
                state.Start(fetchObservables).Wait();
            }
            catch (Exception e)
            {
                caughtException = e;
            }

            caughtException.Should().NotBeNull();
            caughtException.Should().BeAssignableTo(exception.GetType());
        }

        public static IEnumerable<object[]> ExtraExceptionsToRethrow => new[]
        {
            new object[] { new DeserializationException<TestModel>(Substitute.For<IRequest>(), Substitute.For<IResponse>(), "{a:\"b\"}") }
        };
    }
}
