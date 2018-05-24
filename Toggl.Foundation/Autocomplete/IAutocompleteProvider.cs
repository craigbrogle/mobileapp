using System;
using System.Collections.Generic;
using Toggl.Foundation.Autocomplete.Suggestions;

namespace Toggl.Foundation.Autocomplete
{
    public interface IAutocompleteProvider
    {
        IObservable<IEnumerable<AutocompleteSuggestion>> Query(QueryInfo queryInfo);
    }
}
