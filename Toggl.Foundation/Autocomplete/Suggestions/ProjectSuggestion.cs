using System.Collections.Generic;
using System.Linq;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Autocomplete.Suggestions
{
    [Preserve(AllMembers = true)]
    public sealed class ProjectSuggestion : AutocompleteSuggestion
    {
        public const long NoProjectId = 0;

        public static ProjectSuggestion NoProject(long workspaceId, string workspaceName)
            => new ProjectSuggestion(workspaceId, workspaceName);

        public static IEnumerable<ProjectSuggestion> FromProjects(
            IEnumerable<IDatabaseProject> projects
        ) => projects.Select(project => new ProjectSuggestion(project));

        public long ProjectId { get; }

        public int NumberOfTasks { get; } = 0;

        public IList<TaskSuggestion> Tasks { get; }

        public bool HasTasks => Tasks?.Count > 0;

        public string ProjectName { get; } = "";

        public string ClientName { get; } = "";

        public string ProjectColor { get; }

        public bool TasksVisible { get; set; }

        public bool Selected { get; set; }

        private ProjectSuggestion(long workspaceId, string workspaceName)
        {
            ProjectId = NoProjectId;
            ClientName = "";
            ProjectColor = "#A3A3A3";
            ProjectName = Resources.NoProject;
            WorkspaceId = workspaceId;
            WorkspaceName = workspaceName;
        }

        public ProjectSuggestion(IDatabaseProject project)
        {
            ProjectId = project.Id;
            ProjectName = project.Name;
            ProjectColor = project.Color;
            NumberOfTasks = project.Tasks?.Count() ?? 0;
            ClientName = project.Client?.Name ?? "";
            WorkspaceId = project.WorkspaceId;
            WorkspaceName = project.Workspace?.Name ?? "";
            Tasks = project.Tasks?.Select(task => new TaskSuggestion(task)).ToList() ?? new List<TaskSuggestion>();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ProjectName.GetHashCode() * 397) ^
                       (ProjectColor.GetHashCode() * 397) ^
                       ClientName.GetHashCode();
            }
        }
    }
}
