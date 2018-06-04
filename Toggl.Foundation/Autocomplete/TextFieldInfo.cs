using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Foundation.Autocomplete.Suggestions;

namespace Toggl.Foundation.Autocomplete
{
    public struct TextFieldInfo : IEquatable<TextFieldInfo>
    {
        public static TextFieldInfo Empty(long workspaceId) 
            => new TextFieldInfo("", 0, workspaceId, null, "", "", null, "", new TagSuggestion[0]);

        public string Text { get; }

        public int CursorPosition { get; }

        public long WorkspaceId { get; }

        public long? ProjectId { get; }

        public string ProjectColor { get; }

        public string ProjectName { get; }

        public long? TaskId { get; }

        public string TaskName { get; }

        public TagSuggestion[] Tags { get; }

        public int DescriptionCursorPosition
            => Math.Min(CursorPosition, Text.Length);

        private TextFieldInfo(
            string text, 
            int cursorPosition, 
            long workspaceId,
            long? projectId, 
            string projectName, 
            string projectColor,
            long? taskId,
            string taskName,
            TagSuggestion[] tags)
        {
            Text = text;
            Tags = tags;
            TaskId = taskId;
            TaskName = taskName;
            WorkspaceId = workspaceId;
            ProjectId = projectId;
            ProjectName = projectName;
            ProjectColor = projectColor;
            CursorPosition = cursorPosition;
        }

        public TextFieldInfo WithTextAndCursor(string text, int cursorPosition)
            => new TextFieldInfo(text, cursorPosition, WorkspaceId, ProjectId, ProjectName, ProjectColor, TaskId, TaskName, Tags);

        public TextFieldInfo WithWorkspace(long workspaceId)
            => workspaceId == WorkspaceId ? this : new TextFieldInfo(Text, CursorPosition, workspaceId, null, "", "", null, "", new TagSuggestion[0]);

        public TextFieldInfo WithProjectInfo(long workspaceId, long id, string name, string color)
            => new TextFieldInfo(Text, CursorPosition, workspaceId, id, name, color, null, "", Tags);

        public TextFieldInfo WithProjectAndTaskInfo(
            long workspaceId, long projectId, string projectName, string color, long taskId, string taskName)
            => new TextFieldInfo(Text, CursorPosition, workspaceId, projectId, projectName, color, taskId, taskName, Tags);

        public TextFieldInfo RemoveProjectInfo()
            => new TextFieldInfo(Text, CursorPosition, WorkspaceId, null, "", "", null, "", Tags);


        public TextFieldInfo RemoveTagQueryFromDescriptionIfNeeded()
        {
            var indexOfTagQuerySymbol = Text.LastIndexOf(QuerySymbols.Tags);
            if (indexOfTagQuerySymbol < 0) return this;

            var newText = Text.Substring(0, indexOfTagQuerySymbol);
            return WithTextAndCursor(newText, newText.Length);
        }

        public TextFieldInfo RemoveProjectQueryFromDescriptionIfNeeded()
        {
            var indexOfProjectQuerySymbol = Text.LastIndexOf(QuerySymbols.Projects);
            if (indexOfProjectQuerySymbol < 0) return this;

            var newText = Text.Substring(0, indexOfProjectQuerySymbol);
            return WithTextAndCursor(newText, newText.Length);
        }

        public TextFieldInfo AddTag(TagSuggestion tagSuggestion)
        {  
            if (Tags.Any(t => t.TagId == tagSuggestion.TagId))
               return this;

            var tags = new List<TagSuggestion>(Tags) { tagSuggestion }.ToArray();
          
            return new TextFieldInfo(
                Text, CursorPosition, WorkspaceId, ProjectId, ProjectName, ProjectColor, TaskId, TaskName, tags);
        }

        public TextFieldInfo AddTags(TagSuggestion[] tagSuggestions)
        {
            var currentTagIds = Tags.Select(tag => tag.TagId);
            var tagsToAdd = tagSuggestions.Where(tag => !currentTagIds.Contains(tag.TagId));

            return new TextFieldInfo(
                Text, CursorPosition, WorkspaceId, ProjectId, ProjectName, ProjectColor, TaskId, TaskName, Tags.Concat(tagsToAdd).ToArray()
            );
        }

        public TextFieldInfo RemoveTag(TagSuggestion tag)
        {
            var newTags = Tags.ToList();
            newTags.Remove(tag);
            return new TextFieldInfo(
                Text, CursorPosition, WorkspaceId, ProjectId, ProjectName, ProjectColor, TaskId, TaskName, newTags.ToArray());
        }

        public TextFieldInfo ClearTags()
            => new TextFieldInfo(
                Text,
                CursorPosition,
                WorkspaceId,
                ProjectId,
                ProjectName,
                ProjectColor,
                TaskId,
                TaskName,
                new TagSuggestion[0]);
       
        public static bool operator ==(TextFieldInfo left, TextFieldInfo right)
            => left.Equals(right);

        public static bool operator !=(TextFieldInfo left, TextFieldInfo right)
            => !left.Equals(right);

        public override bool Equals(object obj)
            => obj is TextFieldInfo && Equals((TextFieldInfo)obj);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Text.GetHashCode() * 397) ^
                       (CursorPosition.GetHashCode() * 397) ^
                       (ProjectColor.GetHashCode() * 397) ^
                       (Tags.Length.GetHashCode() * 397) ^
                       ProjectName.GetHashCode();
            }
        }

        public bool Equals(TextFieldInfo other)
            => Text == other.Text
            && CursorPosition == other.CursorPosition
            && WorkspaceId == other.WorkspaceId
            && ProjectId == other.ProjectId
            && ProjectName == other.ProjectName
            && ProjectColor == other.ProjectColor
            && Tags == other.Tags;
    }
}
