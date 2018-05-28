using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.Interactors.AutocompleteSuggestions
{
    internal sealed class GetTagsAutocompleteSuggestions : IInteractor<IObservable<IEnumerable<AutocompleteSuggestion>>>
    {
        private readonly ITagsSource dataSource;

        private readonly IEnumerable<string> wordsToQuery;

        public GetTagsAutocompleteSuggestions(ITagsSource dataSource, IEnumerable<string> wordsToQuery)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(wordsToQuery, nameof(wordsToQuery));

            this.dataSource = dataSource;
            this.wordsToQuery = wordsToQuery;
        }

        public IObservable<IEnumerable<AutocompleteSuggestion>> Execute()
            => wordsToQuery
                .Aggregate(dataSource.GetAll(), (obs, word) => obs.Select(filterByWord(word)))
                .Select(TagSuggestion.FromTags);

        private Func<IEnumerable<IThreadSafeTag>, IEnumerable<IThreadSafeTag>> filterByWord(string word)
            => tags => tags.Where(t => t.Name.ContainsIgnoringCase(word));
    }
}
