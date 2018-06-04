using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectableWorkspaceViewModel : MvxNotifyPropertyChanged
    {
        public long WorkspaceId { get; set; }

        public string WorkspaceName { get; set; }

        public bool Selected { get; set; }

        public SelectableWorkspaceViewModel(IThreadSafeWorkspace workspace, bool selected)
        {
            Ensure.Argument.IsNotNull(workspace, nameof(workspace));

            Selected = selected;
            WorkspaceId = workspace.Id;
            WorkspaceName = workspace.Name;
        }
    }
}
