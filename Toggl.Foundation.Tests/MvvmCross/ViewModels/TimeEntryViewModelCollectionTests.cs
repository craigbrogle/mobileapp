using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class TimeEntryViewModelCollectionTests
    {
        public abstract class TimeEntryViewModelCollectionTest : BaseMvvmCrossTests
        {
            protected DateTime Noon = DateTime.Now.Date.AddHours(12);
            protected Subject<DateTimeOffset> TickSubject = new Subject<DateTimeOffset>();
            protected TimeEntryViewModelCollection ViewModel { get; }

            protected TimeEntryViewModelCollectionTest()
            {
                var observable = TickSubject.AsObservable().Publish();
                observable.Connect();
                TimeService.CurrentDateTimeObservable.Returns(observable);

                ViewModel = new TimeEntryViewModelCollection(Noon,
                    Enumerable.Range(0, 10)
                    .Select(i =>
                    {
                        var timeEntry = Substitute.For<IThreadSafeTimeEntry>();
                        timeEntry.Duration.Returns((long)TimeSpan.FromHours(1).TotalSeconds);
                        timeEntry.Start.Returns(Noon.AddHours(-i - 1));
                        return timeEntry;
                    }).Select(te => new TimeEntryViewModel(te, DurationFormat.Improved)).GroupBy(x => x.StartTime.Date).Single(),
                    DurationFormat.Improved
                );
            }
        }

        public sealed class TheConstructor : TimeEntryViewModelCollectionTest
        {
            [Fact, LogIfTooSlow]
            public void ThrowsIfTheArgumentIsNull()
            {
                Action tryingToConstructWithEmptyParameters =
                    () => new TimeEntryViewModelCollection(DateTime.Now, null, DurationFormat.Improved);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }

            [Theory, LogIfTooSlow]
            [InlineData(DateTimeKind.Unspecified)]
            [InlineData(DateTimeKind.Utc)]
            public void ThrowsIfDateKindIsNotLocal(DateTimeKind kind)
            {
                var dateTime = new DateTime(2012, 12, 12, 12, 12, 12, kind);

                Action tryingToConstructWithNonLocalDateTime =
                    () => new TimeEntryViewModelCollection(dateTime, Enumerable.Empty<TimeEntryViewModel>(), DurationFormat.Improved);

                tryingToConstructWithNonLocalDateTime
                    .ShouldThrow<ArgumentException>();
            }
        }

        public sealed class TheCollection : TimeEntryViewModelCollectionTest
        {
            [Fact, LogIfTooSlow]
            public void HasTheSameAmountOfItemsAsThePassedGrouping()
            {
                ViewModel.Should().HaveCount(10);
            }
        }

        public sealed class TheTotalTimeProperty : TimeEntryViewModelCollectionTest
        {
            [Fact, LogIfTooSlow]
            public void EqualsTheSumOfTheDurationOfAllTimeEntries()
            {
                ViewModel.TotalTime.Should().Be(TimeSpan.FromHours(10));
            }
        }
    }
}
