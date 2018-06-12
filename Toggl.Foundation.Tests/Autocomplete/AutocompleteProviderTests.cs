using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Autocomplete;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Interactors;
using Xunit;

namespace Toggl.Foundation.Tests.Autocomplete
{
    public sealed class AutocompleteProviderTests
    {
        public abstract class AutocompleteProviderTest
        {
            protected const long WorkspaceId = 9;
            protected const long ProjectId = 10;
            protected const string ProjectName = "Toggl";
            protected const string ProjectColor = "#F41F19";

            protected AutocompleteProvider Provider { get; }

            protected IInteractorFactory InteractorFactory { get; } = Substitute.For<IInteractorFactory>();

            protected AutocompleteProviderTest()
            {
                Provider = new AutocompleteProvider(InteractorFactory);
            }
        }

        public sealed class TheConstructor
        {
            [Fact, LogIfTooSlow]
            public void ThrowsIfTheArgumentIsNull()
            {
                Action tryingToConstructWithEmptyParameters =
                    // ReSharper disable once ObjectCreationAsStatement
                    () => new AutocompleteProvider(null);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheQueryMethod : AutocompleteProviderTest
        {
            public sealed class UsesGetTimeEntriesAutocompleteSuggestionsInteractor : AutocompleteProviderTest
            {
                [Theory, LogIfTooSlow]
                [InlineData("Nothing")]
                [InlineData("Testing Toggl mobile apps")]
                public async Task WhenTheUserBeginsTypingADescription(string description)
                {
                    var textFieldInfo = TextFieldInfo.Empty(1).WithTextAndCursor(description, 0);

                    await Provider.Query(QueryInfo.ParseFieldInfo(textFieldInfo));

                    InteractorFactory
                        .Received()
                        .GetTimeEntriesAutocompleteSuggestions(Arg.Is<IList<string>>(
                            words => words.SequenceEqual(description.SplitToQueryWords())));
                }

                [Theory, LogIfTooSlow]
                [InlineData("Nothing")]
                [InlineData("Testing Toggl mobile apps")]
                public async Task WhenTheUserHasTypedAnySearchSymbolsButMovedTheCaretToAnIndexThatComesBeforeTheSymbol(
                    string description)
                {
                    var actualDescription = $"{description} @{description}";
                    var textFieldInfo = TextFieldInfo.Empty(1).WithTextAndCursor(actualDescription, 0);

                    await Provider.Query(QueryInfo.ParseFieldInfo(textFieldInfo));

                    InteractorFactory
                        .Received()
                        .GetTimeEntriesAutocompleteSuggestions(Arg.Is<IList<string>>(
                            words => words.SequenceEqual(actualDescription.SplitToQueryWords())));
                }

                [Fact, LogIfTooSlow]
                public async Task WhenTheUserHasAlreadySelectedAProjectAndTypesTheAtSymbol()
                {
                    var description = $"Testing Mobile Apps @toggl";
                    var textFieldInfo = TextFieldInfo.Empty(1)
                        .WithTextAndCursor(description, description.Length)
                        .WithProjectInfo(WorkspaceId, ProjectId, ProjectName, ProjectColor);

                    await Provider.Query(QueryInfo.ParseFieldInfo(textFieldInfo));

                    InteractorFactory
                        .Received()
                        .GetTimeEntriesAutocompleteSuggestions(Arg.Is<IList<string>>(
                            words => words.SequenceEqual(description.SplitToQueryWords())));
                }
            }

            [Theory, LogIfTooSlow]
            [InlineData("Nothing")]
            [InlineData("Testing Toggl mobile apps")]
            public async Task UsesGetProjectsAutocompleteSuggestionsInteractorWhenTheAtSymbolIsTyped(string description)
            {
                var actualDescription = $"{description} @{description}";
                var textFieldInfo = TextFieldInfo.Empty(1).WithTextAndCursor(actualDescription, description.Length + 2);

                await Provider.Query(QueryInfo.ParseFieldInfo(textFieldInfo));

                InteractorFactory
                    .Received()
                    .GetProjectsAutocompleteSuggestions(Arg.Is<IList<string>>(
                        words => words.SequenceEqual(description.SplitToQueryWords())));
            }

            [Theory, LogIfTooSlow]
            [InlineData("Nothing")]
            [InlineData("Testing Toggl mobile apps")]
            public async Task UsesGetTagsAutocompleteSuggestionsInteractorWhenTheHashtagSymbolIsTyped(string description)
            {
                var actualDescription = $"{description} #{description}";
                var textFieldInfo = TextFieldInfo.Empty(1).WithTextAndCursor(actualDescription, description.Length + 2);

                await Provider.Query(QueryInfo.ParseFieldInfo(textFieldInfo));

                InteractorFactory
                    .Received()
                    .GetTagsAutocompleteSuggestions(Arg.Is<IList<string>>(
                        words => words.SequenceEqual(description.SplitToQueryWords())));
            }

            [Fact, LogIfTooSlow]
            public async Task DoesNotUseInteractorsWhenTheSarchStringIsEmpty()
            {
                var textFieldInfo = TextFieldInfo.Empty(1).WithTextAndCursor("", 0);

                await Provider.Query(QueryInfo.ParseFieldInfo(textFieldInfo));

                InteractorFactory
                    .DidNotReceive()
                    .GetTagsAutocompleteSuggestions(Arg.Any<IList<string>>());
                InteractorFactory
                    .DidNotReceive()
                    .GetProjectsAutocompleteSuggestions(Arg.Any<IList<string>>());
                InteractorFactory
                    .DidNotReceive()
                    .GetTimeEntriesAutocompleteSuggestions(Arg.Any<IList<string>>());
            }
        }
    }
}
