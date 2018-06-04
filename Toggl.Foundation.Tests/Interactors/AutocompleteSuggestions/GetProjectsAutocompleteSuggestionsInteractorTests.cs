using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Interactors.AutocompleteSuggestions;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant.Models;
using Xunit;

namespace Toggl.Foundation.Tests.Interactors.AutocompleteSuggestions
{
    public sealed class GetProjectsAutocompleteSuggestionsInteractorTests : BaseAutocompleteSuggestionsInteractorTest
    {
        private readonly IProjectsSource dataSource = Substitute.For<IProjectsSource>();

        public GetProjectsAutocompleteSuggestionsInteractorTests()
        {
            dataSource.GetAll(Arg.Any<Func<IDatabaseProject, bool>>())
                .Returns(callInfo => Observable.Return(Projects.Where(callInfo.Arg<Func<IThreadSafeProject, bool>>())));
        }

        [Fact, LogIfTooSlow]
        public async Task SearchesTheName()
        {
            var interactor = new GetProjectsAutocompleteSuggestions(dataSource, new[] { "30" });

            var suggestions = await interactor.Execute();

            suggestions.Should().HaveCount(1)
                .And.AllBeOfType<ProjectSuggestion>();
        }

        [Fact, LogIfTooSlow]
        public async Task SearchesTheClientsName()
        {
            var interactor = new GetProjectsAutocompleteSuggestions(dataSource, new[] { "10" });

            var suggestions = await interactor.Execute();

            suggestions.Should().HaveCount(1)
                .And.AllBeOfType<ProjectSuggestion>();
        }

        [Fact, LogIfTooSlow]
        public async Task SearchesTheTaskName()
        {
            var interactor = new GetProjectsAutocompleteSuggestions(dataSource, new[] { "20" });

            var suggestions = await interactor.Execute();

            suggestions.Should().HaveCount(1)
                .And.AllBeOfType<ProjectSuggestion>();
        }

        [Fact, LogIfTooSlow]
        public async Task OnlyDisplaysResultsTheHaveHasAtLeastOneMatchOnEveryWordTyped()
        {
            var interactor = new GetProjectsAutocompleteSuggestions(dataSource, new[] { "10", "3" });

            var suggestions = await interactor.Execute();

            suggestions.Should().HaveCount(1)
                .And.AllBeOfType<ProjectSuggestion>();
        }


        [Fact, LogIfTooSlow]
        public async Task ReturnsAllProjectsWhenThereAreNoWordsToFilter()
        {
            var interactor = new GetProjectsAutocompleteSuggestions(dataSource, new string[0]);

            var suggestions = await interactor.Execute();

            suggestions.Should().HaveCount(Projects.Count(p => p.Active))
                .And.AllBeOfType<ProjectSuggestion>();
        }

        [Fact, LogIfTooSlow]
        public async Task ReturnsOnlyActiveProjects()
        {
            var interactor = new GetProjectsAutocompleteSuggestions(dataSource, new string[0]);

            var suggestions = await interactor.Execute().SelectMany(s => s).ToList();

            suggestions
                .Select(s => ((ProjectSuggestion)s).ProjectId)
                .SelectMany(id => Projects.Where(t => t.Id == id))
                .ForEach(project => project.Active.Should().BeTrue());
        }
    }
}
