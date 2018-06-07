using System;
using System.Linq;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Autocomplete;
using Toggl.Foundation.Autocomplete.Suggestions;

namespace Toggl.Foundation.MvvmCross.Extensions
{
    public static class BundleExtensions
    {
        public static void SavePropertiesFrom(this IMvxBundle bundle, StartTimeEntryViewModel viewModel)
        {
            var package = new BundlePackage<StartTimeEntryViewModel>(bundle, viewModel);

            package.Store(vm => vm.TextFieldInfo.Text);
            package.Store(vm => vm.TextFieldInfo.CursorPosition);
            package.Store(vm => vm.TextFieldInfo.WorkspaceId);

            package.Store(vm => vm.TextFieldInfo.WorkspaceId);

            var hasProject = viewModel.TextFieldInfo.ProjectId.HasValue;
            package.Store(vm => vm.TextFieldInfo.ProjectId.HasValue, hasProject);
            if (hasProject)
            {
                package.Store(vm => vm.TextFieldInfo.ProjectId);
                package.Store(vm => vm.TextFieldInfo.ProjectColor);
                package.Store(vm => vm.TextFieldInfo.ProjectName);
            }

            var hasTask = viewModel.TextFieldInfo.TaskId.HasValue;
            package.Store(vm => vm.TextFieldInfo.TaskId.HasValue, hasTask);
            if (hasTask)
            {
                package.Store(vm => vm.TextFieldInfo.TaskId);
                package.Store(vm => vm.TextFieldInfo.TaskName);
            }

            var hasTags = viewModel.TextFieldInfo.Tags.Length > 0;
            package.Store(vm => vm.TextFieldInfo.Tags.Length, hasTags);
            if (hasTags)
            {
                var tagIds = viewModel.TextFieldInfo.Tags.Select(tag => tag.TagId);
                package.Store(vm => vm.TextFieldInfo.Tags, tagIds);
            }

            package.Store(vm => vm.StartTime);
            package.Store(vm => vm.IsBillable);
        }

        public static void ReloadPropertiesInto(this IMvxBundle bundle, StartTimeEntryViewModel viewModel)
        {
            var package = new BundlePackage<StartTimeEntryViewModel>(bundle, viewModel);

            if (!package.TryGetValue(vm => vm.TextFieldInfo.Text, out string text)
                || !package.TryGetValue(vm => vm.TextFieldInfo.CursorPosition, out int cursorPosition)
                || !package.TryGetValue(vm => vm.TextFieldInfo.WorkspaceId, out long workspaceId)
                || !package.TryGetValue(vm => vm.StartTime, out DateTimeOffset startTime)
                || !package.TryGetValue(vm => vm.IsBillable, out bool isBillable))
            {
                return;
            }

            var textFieldInfo = TextFieldInfo.Empty(workspaceId).WithTextAndCursor(text, cursorPosition);

            package.TryGetValue(vm => vm.TextFieldInfo.ProjectId.HasValue, out bool hasProject);
            package.TryGetValue(vm => vm.TextFieldInfo.TaskId.HasValue, out bool hasTask);

            if (hasProject)
            {
                if (!package.TryGetValue(vm => vm.TextFieldInfo.ProjectId, out int projectId)
                    || !package.TryGetValue(vm => vm.TextFieldInfo.ProjectColor, out string projectColor)
                    || !package.TryGetValue(vm => vm.TextFieldInfo.ProjectName, out string projectName))
                {
                    return;
                }

                if (hasTask)
                {
                    if (!package.TryGetValue(vm => vm.TextFieldInfo.TaskId, out int taskId)
                        || !package.TryGetValue(vm => vm.TextFieldInfo.TaskName, out string taskName))
                    {
                        return;
                    }

                    textFieldInfo = textFieldInfo.WithProjectAndTaskInfo(workspaceId, projectId, projectName, projectColor, taskId, taskName);
                }
                else
                {
                    textFieldInfo = textFieldInfo.WithProjectInfo(workspaceId, projectId, projectName, projectColor);
                }
            }

            package.TryGetValue(vm => vm.TextFieldInfo.Tags.Length, out bool hasTags);
            bool hasIdsString = package.TryGetValue(vm => vm.TextFieldInfo.Tags, out string tagsIdsString);

            if (hasTags && hasIdsString)
            {
                var tagIds = tagsIdsString.Split(';').Select(str => long.Parse(str)).ToArray();
                viewModel.TagIdsToReload = tagIds;
            }

            viewModel.TextFieldInfo = textFieldInfo;
            viewModel.StartTime = startTime;
            viewModel.IsBillable = isBillable;
        }
    }
}
