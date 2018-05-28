using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Interactors;
using Toggl.Multivac;

namespace Toggl.Foundation.Autocomplete
{
    [Preserve(AllMembers = true)]
    public sealed class AutocompleteProvider : IAutocompleteProvider
    {
        private readonly IInteractorFactory interactorFactory;

        public AutocompleteProvider(IInteractorFactory interactorFactory)
        {
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));

            this.interactorFactory = interactorFactory;
        }

        public IObservable<IEnumerable<AutocompleteSuggestion>> Query(QueryInfo queryInfo)
        {
            var wordsToQuery = queryInfo.Text.SplitToQueryWords();
            switch (queryInfo.SuggestionType)
            {
                case AutocompleteSuggestionType.Projects:
                    return interactorFactory.GetProjectsAutocompleteSuggestions(wordsToQuery).Execute();

                case AutocompleteSuggestionType.Tags:
                    return interactorFactory.GetTagsAutocompleteSuggestions(wordsToQuery).Execute();
            }

            return wordsToQuery.Count == 0
                ? Observable.Return(QuerySymbolSuggestion.Suggestions)
                : interactorFactory.GetTimeEntriesAutocompleteSuggestions(wordsToQuery).Execute();
        }
    }
}
