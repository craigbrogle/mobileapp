using Android.Widget;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using static Toggl.Giskard.Resource.Id;
using static Toggl.Foundation.MvvmCross.Parameters.SelectTimeParameters.Origin;

namespace Toggl.Giskard.Activities
{
    public partial class StartTimeEntryActivity
    {
        private TextView durationLabel;

        private void initializeViews()
        {
            durationLabel = FindViewById(StartTimeEntryDurationText);
        }

        private void setupBindings() 
        {
            this.Bind(durationLabel.Tapped(), _ => ViewModel.SelectTimeCommand.Execute(Duration));
        }
    }
}
