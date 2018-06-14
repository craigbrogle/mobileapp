using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Xunit;

namespace Toggl.Foundation.Tests.AnalyticsService
{
    public sealed class TrackableEventsTests
    {
        private const string OverriddenEventName = "Overridden";
        private const string OverriddenParameterName = "CreationOrigin";

        private class RandomEvent : ITrackableEvent
        {
            public int Ergalactic { get; } = 199;
            public string Theory { get; } = "Quantum entanglement";
            public bool George { get; } = true;
            public DateTimeOffset Time { get; } = DateTimeOffset.Parse("2016-12-08 04:00");
        }

        [DisplayName(OverriddenEventName)]
        private class StartTimeEntryEvent : ITrackableEvent
        {
            [DisplayName(OverriddenParameterName)]
            public TimeEntryStartOrigin Origin { get; } = TimeEntryStartOrigin.Timer;

            public bool IsDescriptionNotEmpty { get; } = true;
            public bool HasProject { get; } = true;
            public bool HasTask { get; } = false;
            public int TagCount { get; } = 42;
            public bool IsRunning { get; } = false;
            public bool IsBillable { get; } = false;
        }

        public IAnalyticsService AnalyticsService { get; } = Substitute.For<IAnalyticsService>();

        private RandomEvent createRandomEvent() => new RandomEvent();
        private StartTimeEntryEvent createStartTimeEntryEvent() => new StartTimeEntryEvent();

        public TrackableEventsTests()
        {
            AnalyticsService = Substitute.For<IAnalyticsService>();
        }

        [Fact]
        public void CorrectlyTracksAnObject()
        {
            var randomEvent = createRandomEvent();

            randomEvent.TrackWith(AnalyticsService);

            AnalyticsService.Received().Track(nameof(RandomEvent),
                Arg.Is<Dictionary<string, string>>(dict =>
                                                   dict[nameof(randomEvent.Ergalactic)] == randomEvent.Ergalactic.ToString() &&
                                                   dict[nameof(randomEvent.Theory)] == randomEvent.Theory.ToString() &&
                                                   dict[nameof(randomEvent.George)] == randomEvent.George.ToString() &&
                                                   dict[nameof(randomEvent.Time)] == randomEvent.Time.ToString()
            ));
        }

        [Fact]
        public void UsesOverridenEventName()
        {
            var randomEvent = createStartTimeEntryEvent();

            randomEvent.TrackWith(AnalyticsService);

            AnalyticsService.Received().Track(OverriddenEventName, Arg.Any<Dictionary<string, string>>());
        }

        [Fact]
        public void UsesOverridenParameterName()
        {
            var randomEvent = createStartTimeEntryEvent();

            randomEvent.TrackWith(AnalyticsService);

            AnalyticsService.Received().Track(Arg.Any<string>(),
                Arg.Is<Dictionary<string, string>>(d => d[OverriddenParameterName] == TimeEntryStartOrigin.Timer.ToString()
            ));
        }
    }
}
