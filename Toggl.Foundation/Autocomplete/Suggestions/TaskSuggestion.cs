using Toggl.Foundation.Extensions;
using Toggl.Foundation.Models.Interfaces;

namespace Toggl.Foundation.Autocomplete.Suggestions
{
    public sealed class TaskSuggestion : AutocompleteSuggestion
    {
        public long TaskId { get; }

        public string Name { get; }

        public long ProjectId { get; }

        public string ProjectName { get; }

        public string ProjectColor { get; }

        public TaskSuggestion(IThreadSafeTask task)
        {
            TaskId = task.Id;
            Name = task.Name;
            ProjectId = task.ProjectId;
            WorkspaceId = task.WorkspaceId;
            ProjectName = task.Project?.DisplayName() ?? "";
            ProjectColor = task.Project?.DisplayColor() ?? "";
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (TaskId.GetHashCode() * 397) ^
                       (ProjectId.GetHashCode() * 397) ^
                        Name.GetHashCode();
            }
        }
    }
}
