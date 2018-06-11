using System;
using FluentAssertions;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Tests.Helpers;
using Toggl.Ultrawave.Exceptions;
using Toggl.Ultrawave.Serialization;
using Xunit;

namespace Toggl.Foundation.Tests.Extensions
{
    public sealed class ExceptionExtensionsTests
    {
        [Fact]
        public void MarksSerializationExceptionAsNotAnonymized()
        {
            var exception = new SerializationException(typeof(Ultrawave.Models.User), new Exception());

            var isAnonymized = exception.IsAnonymized();

            isAnonymized.Should().BeFalse();
        }

        [Theory]
        [MemberData(nameof(ApiExceptions.ClientExceptions), MemberType = typeof(ApiExceptions))]
        [MemberData(nameof(ApiExceptions.ServerExceptions), MemberType = typeof(ApiExceptions))]
        public void MarksApiExceptionsAsAnonymized(ApiException exception)
        {
            var isAnonymized = exception.IsAnonymized();

            isAnonymized.Should().BeTrue();
        }
    }
}
