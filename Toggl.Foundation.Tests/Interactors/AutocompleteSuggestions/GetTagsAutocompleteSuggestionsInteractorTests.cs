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
    public sealed class GetTagsAutocompleteSuggestionsInteractorTests : BaseAutocompleteSuggestionsInteractorTest
    {
        private readonly ITagsSource dataSource = Substitute.For<ITagsSource>();

        public GetTagsAutocompleteSuggestionsInteractorTests()
        {
            dataSource.GetAll().Returns(Observable.Return(Tags));
        }

        [Fact, LogIfTooSlow]
        public async Task SuggestsAllTagsWhenThereIsNoStringToSearch()
        {
            var interactor = new GetTagsAutocompleteSuggestions(dataSource, new string[0]);

            var suggestions = await interactor.Execute();

            suggestions.Should().HaveCount(Tags.Count())
                .And.AllBeOfType<TagSuggestion>();
        }

        [Fact, LogIfTooSlow]
        public async Task SearchesTheName()
        {
            var interactor = new GetTagsAutocompleteSuggestions(dataSource, new[] { "50" });

            var suggestions = await interactor.Execute();

            suggestions.Should().HaveCount(1)
                .And.AllBeOfType<TagSuggestion>();
        }

        [Fact, LogIfTooSlow]
        public async Task OnlyDisplaysResultsThatHaveAtLeastOneMatchOnEveryWordTyped()
        {
            var interactor = new GetTagsAutocompleteSuggestions(dataSource, new[] { "5", "2" });

            var suggestions = await interactor.Execute();

            suggestions.Single().Should().BeOfType<TagSuggestion>()
                .Which.Name.Should().Be("52");
        }
    }
}
