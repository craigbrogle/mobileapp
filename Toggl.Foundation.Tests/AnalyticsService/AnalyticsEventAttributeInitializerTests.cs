using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Xunit;

namespace Toggl.Foundation.Tests.AnalyticsService
{
    public sealed class AnalyticsEventAttributeInitializerTests
    {
        private abstract class BaseTestClassWithAttributes : AnalyticsEventAttributeInitializer
        {
            public IAnalyticsService AnalyticsService { get; } = Substitute.For<IAnalyticsService>();

            public BaseTestClassWithAttributes()
            {
                InitializeAttributedProperties(AnalyticsService);
            }
        }

        private class TestClassWithAttributes : BaseTestClassWithAttributes
        {
            [AnalyticsEvent]
            public IAnalyticsEvent EventWithoutParameters { get; protected set; }

            [AnalyticsEvent("NameOfParameter")]
            public IAnalyticsEvent<int> EventWithOneParameter { get; protected set; }

            [AnalyticsEvent("NameOfParameterA", "NameOfParameterB")]
            public IAnalyticsEvent<string, DateTimeOffset> EventWithTwoParameters { get; protected set; }

            [AnalyticsEvent("NameOfParameter1", "NameOfParameter2", "NameOfParameter3")]
            public IAnalyticsEvent<long, bool, object> EventWithThreeParameters { get; protected set; }

            [AnalyticsEvent("NameOfParameterI", "NameOfParameterII", "NameOfParameterIII", "NameOfParameterIV")]
            public IAnalyticsEvent<long, bool, object, Exception> EventWithFourParameters { get; protected set; }
        }

        private class TestClassWithZeroParameterAttributeWhichThrows : BaseTestClassWithAttributes
        {
            [AnalyticsEvent("NameOfParameter1")]
            public IAnalyticsEvent EventWithParameters { get; protected set; }
        }

        private class TestClassWithOneParameterAttributeWhichThrows : BaseTestClassWithAttributes
        {
            [AnalyticsEvent]
            public IAnalyticsEvent<long> EventWithParameters { get; protected set; }
        }

        private class TestClassWithTwoParameterAttributeWhichThrows : BaseTestClassWithAttributes
        {
            [AnalyticsEvent("NameOfParameter1", "NameOfParameter2", "NameOfParameter3")]
            public IAnalyticsEvent<long, bool> EventWithParameters { get; protected set; }
        }

        private class TestClassWithThreeParameterAttributeWhichThrows : BaseTestClassWithAttributes
        {
            [AnalyticsEvent("NameOfParameter1", "NameOfParameter2", "NameOfParameter3", "NameOfParameter4")]
            public IAnalyticsEvent<long, bool, object> EventWithParameters { get; protected set; }
        }

        private class TestClassWithFourParameterAttributeWhichThrows : BaseTestClassWithAttributes
        {
            [AnalyticsEvent("NameOfParameter1", "NameOfParameter2")]
            public IAnalyticsEvent<long, bool, object, Exception> EventWithParameters { get; protected set; }
        }

        [Fact]
        public void InitializesAllProperties()
        {
            var instance = new TestClassWithAttributes();

            instance.EventWithoutParameters.Should().NotBeNull();
            instance.EventWithOneParameter.Should().NotBeNull();
            instance.EventWithTwoParameters.Should().NotBeNull();
            instance.EventWithThreeParameters.Should().NotBeNull();
            instance.EventWithFourParameters.Should().NotBeNull();
        }

        [Fact]
        public void InitializesAllPropertiesToAnalyticsEventInstancesWithCorrectGenerics()
        {
            var instance = new TestClassWithAttributes();

            instance.EventWithoutParameters.Should().BeOfType<AnalyticsEvent>();
            instance.EventWithOneParameter.Should().BeOfType<AnalyticsEvent<int>>();
            instance.EventWithTwoParameters.Should().BeOfType<AnalyticsEvent<string, DateTimeOffset>>();
            instance.EventWithThreeParameters.Should().BeOfType<AnalyticsEvent<long, bool, object>>();
            instance.EventWithFourParameters.Should().BeOfType<AnalyticsEvent<long, bool, object, Exception>>();
        }

        [Fact]
        public void TracksTheEventWithCorrectNameWithoutAnyParameters()
        {
            var instance = new TestClassWithAttributes();

            instance.EventWithoutParameters.Track();

            instance.AnalyticsService.Received().Track(
                "EventWithoutParameters",
                Arg.Is<Dictionary<string, string>>(dict => dict.Count == 0));
        }

        [Fact]
        public void TracksTheEventWithCorrectNameAndWithOneNamedParameter()
        {
            var instance = new TestClassWithAttributes();

            instance.EventWithOneParameter.Track(123);

            instance.AnalyticsService.Received().Track(
                "EventWithOneParameter",
                Arg.Is<Dictionary<string, string>>(dict =>
                    dict.ContainsKey("NameOfParameter")
                    && dict["NameOfParameter"] == "123"));
        }

        [Fact]
        public void TracksTheEventWithCorrectNameAndWithTwoNamedParameters()
        {
            var date = new DateTimeOffset(2018, 01, 01, 12, 34, 56, TimeSpan.FromHours(4));
            var instance = new TestClassWithAttributes();

            instance.EventWithTwoParameters.Track("abcde", date);

            instance.AnalyticsService.Received().Track(
                "EventWithTwoParameters",
                Arg.Is<Dictionary<string, string>>(dict =>
                    dict.ContainsKey("NameOfParameterA")
                    && dict.ContainsKey("NameOfParameterB")
                    && dict["NameOfParameterA"] == "abcde"
                    && dict["NameOfParameterB"] == date.ToString()));
        }

        [Fact]
        public void TracksTheEventWithCorrectNameAndWithThreeNamedParameters()
        {
            var instance = new TestClassWithAttributes();

            instance.EventWithThreeParameters.Track(123L, true, string.Empty);

            instance.AnalyticsService.Received().Track(
                "EventWithThreeParameters",
                Arg.Is<Dictionary<string, string>>(dict =>
                    dict.ContainsKey("NameOfParameter1")
                    && dict.ContainsKey("NameOfParameter2")
                    && dict.ContainsKey("NameOfParameter3")
                    && dict["NameOfParameter1"] == "123"
                    && dict["NameOfParameter2"] == "True"
                    && dict["NameOfParameter3"] == string.Empty));
        }

        [Fact]
        public void TracksTheEventWithCorrectNameAndWithFourNamedParameters()
        {
            var exception = new InvalidOperationException();
            var instance = new TestClassWithAttributes();

            instance.EventWithFourParameters.Track(123L, true, string.Empty, exception);

            instance.AnalyticsService.Received().Track(
                "EventWithFourParameters",
                Arg.Is<Dictionary<string, string>>(dict =>
                    dict.ContainsKey("NameOfParameterI")
                    && dict.ContainsKey("NameOfParameterII")
                    && dict.ContainsKey("NameOfParameterIII")
                    && dict.ContainsKey("NameOfParameterIV")
                    && dict["NameOfParameterI"] == "123"
                    && dict["NameOfParameterII"] == "True"
                    && dict["NameOfParameterIII"] == string.Empty
                    && dict["NameOfParameterIV"] == exception.ToString()));
        }

        [Theory]
        [MemberData(nameof(IncorrectAttributesTestData))]
        public void ThrowsIfTheNumberOfAttributeParametersDoesNotMatchTheNumberOfGenerics(Action creatingInstance)
        {
            creatingInstance.Should().Throw<InvalidOperationException>();
        }

        public static IEnumerable<object[]> IncorrectAttributesTestData()
        {
            yield return new Action[] { () => new TestClassWithZeroParameterAttributeWhichThrows() };
            yield return new Action[] { () => new TestClassWithOneParameterAttributeWhichThrows() };
            yield return new Action[] { () => new TestClassWithTwoParameterAttributeWhichThrows() };
            yield return new Action[] { () => new TestClassWithThreeParameterAttributeWhichThrows() };
            yield return new Action[] { () => new TestClassWithFourParameterAttributeWhichThrows() };
        }
    }
}
