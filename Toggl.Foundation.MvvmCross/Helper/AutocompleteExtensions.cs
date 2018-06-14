using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.MvvmCross.Collections;

namespace Toggl.Foundation.MvvmCross.Helper
{
    using WorkspaceGroupedSuggestionsCollection = WorkspaceGroupedCollection<AutocompleteSuggestion>;
    using WorkspaceSuggestionsGrouping = IGrouping<(long workspaceId, string workspaceName), AutocompleteSuggestion>;

    internal static class AutocompleteExtensions
    {
        public static IEnumerable<WorkspaceGroupedSuggestionsCollection> GroupByWorkspace(
            this IEnumerable<AutocompleteSuggestion> suggestions)
            => suggestions
                .Where(suggestion => !string.IsNullOrEmpty(suggestion.WorkspaceName))
                .GroupBy(suggestion => (suggestion.WorkspaceId, suggestion.WorkspaceName))
                .Select(createWorkspaceGroupedCollection);

        public static IEnumerable<WorkspaceGroupedSuggestionsCollection> GroupByWorkspaceAddingNoProject(
            this IEnumerable<AutocompleteSuggestion> suggestions)
            => suggestions
                .GroupByWorkspace()
                .Select(withNoProject);

        public static IEnumerable<WorkspaceGroupedSuggestionsCollection> OrderByDefaultWorkspaceAndName(
            this IEnumerable<WorkspaceGroupedSuggestionsCollection> suggestions, long defaultWorkSpaceId)
        {
            return suggestions
                .OrderByDescending(suggestion => suggestion.WorkspaceId == defaultWorkSpaceId)
                .ThenBy(s => s.WorkspaceName);
        }

        private static WorkspaceGroupedSuggestionsCollection withNoProject(WorkspaceGroupedSuggestionsCollection collection)
        {
            collection.Insert(0, ProjectSuggestion.NoProject(collection.WorkspaceId, collection.WorkspaceName));
            return collection;
        }

        private static WorkspaceGroupedSuggestionsCollection createWorkspaceGroupedCollection(WorkspaceSuggestionsGrouping grouping)
            => new WorkspaceGroupedSuggestionsCollection(grouping.Key.workspaceName, grouping.Key.workspaceId, grouping);
    }
}
