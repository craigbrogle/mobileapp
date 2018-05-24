using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Interactors.AutocompleteSuggestions;
using Xunit;

namespace Toggl.Foundation.Tests.Interactors.AutocompleteSuggestions
{
    public sealed class GetTimeEntriesAutocompleteSuggestionsInteractorTests : BaseAutocompleteSuggestionsInteractorTest
    {
        private readonly ITimeEntriesSource dataSource = Substitute.For<ITimeEntriesSource>();

        public GetTimeEntriesAutocompleteSuggestionsInteractorTests()
        {
            var nonDeletedTimeEntries = TimeEntries.Where(te => te.IsDeleted == false);
            dataSource.GetAll().Returns(Observable.Return(nonDeletedTimeEntries));
        }

        [Fact, LogIfTooSlow]
        public async Task SearchesTheDescription()
        {
            var interactor = new GetTimeEntriesAutocompleteSuggestions(dataSource, new[] { "40" });

            var suggestions = await interactor.Execute();

            suggestions.Should().HaveCount(1)
                .And.AllBeOfType<TimeEntrySuggestion>();
        }

        [Fact, LogIfTooSlow]
        public async Task SearchesTheProjectsName()
        {
            var interactor = new GetTimeEntriesAutocompleteSuggestions(dataSource, new[] { "30" });

            var suggestions = await interactor.Execute();

            suggestions.Should().HaveCount(1)
                .And.AllBeOfType<TimeEntrySuggestion>();
        }

        [Fact, LogIfTooSlow]
        public async Task SearchesTheClientsName()
        {
            var interactor = new GetTimeEntriesAutocompleteSuggestions(dataSource, new[] { "10" });

            var suggestions = await interactor.Execute();

            suggestions.Should().HaveCount(1)
                .And.AllBeOfType<TimeEntrySuggestion>();
        }

        [Fact, LogIfTooSlow]
        public async Task SearchesTheTaskName()
        {
            var interactor = new GetTimeEntriesAutocompleteSuggestions(dataSource, new[] { "25" });

            var suggestions = await interactor.Execute().SelectMany(s => s).ToList();

            suggestions.Should().HaveCount(1)
                .And.AllBeOfType<TimeEntrySuggestion>();
            var suggestion = (TimeEntrySuggestion)suggestions.First();
            suggestion.TaskId.Should().Be(25);
            suggestion.ProjectId.Should().Be(36);
            suggestion.Description.Should().Be("46");
        }

        [Fact, LogIfTooSlow]
        public async Task OnlyDisplaysResultsTheHaveHasAtLeastOneMatchOnEveryWordTyped()
        {
            var interactor = new GetTimeEntriesAutocompleteSuggestions(dataSource, new[] { "10", "30", "4" });

            var suggestions = await interactor.Execute();

            suggestions.Should().HaveCount(1)
                .And.AllBeOfType<TimeEntrySuggestion>();
        }

        [Fact, LogIfTooSlow]
        public async Task DoNotSuggestDeletedTimeEntriesAmongFilteredTimeEntries()
        {
            var interactor = new GetTimeEntriesAutocompleteSuggestions(dataSource, new[] { "49" });

            var suggestions = await interactor.Execute();

            suggestions.Should().HaveCount(0)
                .And.AllBeOfType<TimeEntrySuggestion>();
        }

        [Fact, LogIfTooSlow]
        public async Task DoNotSuggestTimeEntriesWhichReferenceArchivedProjects()
        {
            var interactor = new GetTimeEntriesAutocompleteSuggestions(dataSource, new[] { "38" });

            var suggestions = await interactor.Execute();

            suggestions.Should().HaveCount(0);
        }
    }
}
