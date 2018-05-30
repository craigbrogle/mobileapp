using System;
using System.Linq;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Autocomplete;

namespace Toggl.Foundation.MvvmCross.Extensions
{
    public static class BundleExtensions
    {
        public static void SavePropertiesFrom(this IMvxBundle bundle, StartTimeEntryViewModel viewModel)
        {
            bundle.Data[$"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.Text)}"] = viewModel.TextFieldInfo.Text;
            bundle.Data[$"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.CursorPosition)}"] = viewModel.TextFieldInfo.CursorPosition.ToString();
            bundle.Data[$"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.WorkspaceId)}"] = viewModel.TextFieldInfo.WorkspaceId.ToString();

            var hasProject = viewModel.TextFieldInfo.ProjectId.HasValue;
            bundle.Data[$"{nameof(TextFieldInfo)}.{nameof(viewModel.TextFieldInfo.ProjectId.HasValue)}"] = hasProject.ToString();
            if (hasProject)
            {
                bundle.Data[$"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.ProjectId)}"] = viewModel.TextFieldInfo.ProjectId.ToString();
                bundle.Data[$"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.ProjectColor)}"] = viewModel.TextFieldInfo.ProjectColor;
                bundle.Data[$"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.ProjectName)}"] = viewModel.TextFieldInfo.ProjectName;
            }

            var hasTask = viewModel.TextFieldInfo.TaskId.HasValue;
            bundle.Data[$"{nameof(TextFieldInfo)}.{nameof(viewModel.TextFieldInfo.TaskId.HasValue)}"] = hasTask.ToString();
            if (hasTask)
            {
                bundle.Data[$"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.TaskId)}"] = viewModel.TextFieldInfo.TaskId.ToString();
                bundle.Data[$"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.TaskName)}"] = viewModel.TextFieldInfo.TaskName;
            }

            var hasTags = viewModel.TextFieldInfo.Tags.Length > 0;
            bundle.Data[$"{nameof(TextFieldInfo)}.{nameof(viewModel.TextFieldInfo.Tags.Length)}"] = hasTags.ToString();
            if (hasTags)
            {
                var tagIds = viewModel.TextFieldInfo.Tags.Select(tag => tag.TagId);
                bundle.Data[$"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.Tags)}"] = string.Join(";", tagIds);
            }

            bundle.Data[nameof(viewModel.StartTime)] = viewModel.StartTime.ToString();
            bundle.Data[nameof(viewModel.IsBillable)] = viewModel.IsBillable.ToString();
        }

        public static void ReloadPropertiesInto(this IMvxBundle bundle, StartTimeEntryViewModel viewModel)
        {
            if (bundle.Data.TryGetValue($"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.Text)}", out var text) &&

                bundle.Data.TryGetValue($"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.CursorPosition)}", out var cursorPositionString) &&
                int.TryParse(cursorPositionString, out var cursorPosition) &&

                bundle.Data.TryGetValue($"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.WorkspaceId)}", out var workspaceIdString) &&
                long.TryParse(workspaceIdString, out var workspaceId) &&

                bundle.Data.TryGetValue($"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.ProjectId)}", out var hasProjectString) &&
                bool.TryParse(hasProjectString, out var hasProject) &&

                bundle.Data.TryGetValue($"{nameof(TextFieldInfo)}.{nameof(viewModel.TextFieldInfo.TaskId.HasValue)}", out var hasTaskString) &&
                bool.TryParse(hasTaskString, out var hasTask) &&

                bundle.Data.TryGetValue($"{nameof(TextFieldInfo)}.{nameof(viewModel.TextFieldInfo.Tags.Length)}", out var hasTagsString) &&
                bool.TryParse(hasTagsString, out var hasTags) &&

                bundle.Data.TryGetValue(nameof(viewModel.StartTime), out var startTimeString) &&
                DateTimeOffset.TryParse(startTimeString, out var startTime) &&

                bundle.Data.TryGetValue(nameof(viewModel.IsBillable), out var isBillableString) &&
                bool.TryParse(isBillableString, out var isBillable))
            {
                var textFieldInfo = TextFieldInfo
                    .Empty(workspaceId)
                    .WithTextAndCursor(text, cursorPosition);

                if (hasProject)
                {
                    if (bundle.Data.TryGetValue($"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.ProjectId)}", out var projectIdString) &&
                        int.TryParse(projectIdString, out var projectId) &&
                        bundle.Data.TryGetValue($"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.ProjectColor)}", out var projectColor) &&
                        bundle.Data.TryGetValue($"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.ProjectName)}", out var projectName))
                    {
                        if (hasTask)
                        {
                            if (bundle.Data.TryGetValue($"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.TaskId)}", out var taskIdString) &&
                                int.TryParse(taskIdString, out var taskId) &&
                                bundle.Data.TryGetValue($"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.TaskName)}", out var taskName))
                            {
                                textFieldInfo = textFieldInfo.WithProjectAndTaskInfo(workspaceId, projectId, projectName, projectColor, taskId, taskName);
                            }
                            else
                            {
                                textFieldInfo = textFieldInfo.WithProjectInfo(workspaceId, projectId, projectName, projectColor);
                            }
                        }
                    } else
                    {
                        return;
                    }
                }

                if (hasTags)
                {
                    if (bundle.Data.TryGetValue($"{nameof(TextFieldInfo)}.{nameof(TextFieldInfo.Tags)}", out var tagsIdsString))
                    {
                        var tagIds = tagsIdsString.Split(';').Select(str => long.Parse(str)).ToArray();
                        viewModel.TagIds = tagIds;
                    }
                }

                viewModel.TextFieldInfo = textFieldInfo;
                viewModel.StartTime = startTime;
                viewModel.IsBillable = isBillable;
            }
        }
    }
}
