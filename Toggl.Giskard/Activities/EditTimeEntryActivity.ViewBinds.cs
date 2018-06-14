using System;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using Toggl.Giskard.Fragments;
using Toggl.Giskard.Views;
using static Android.Support.V7.Widget.Toolbar;
using TextView = Android.Widget.TextView;
using static Toggl.Giskard.Resource.Id;
using static Toggl.Foundation.MvvmCross.Parameters.SelectTimeParameters.Origin;

namespace Toggl.Giskard.Activities
{
    public sealed partial class EditTimeEntryActivity : MvxAppCompatActivity<EditTimeEntryViewModel>
    {
        private View startTimeArea;
        private View stopTimeArea;
        private View durationArea;

        private void initializeViews()
        {
            startTimeArea = FindViewById(EditTimeLeftPart);
            stopTimeArea = FindViewById(EditTimeRightPart);
            durationArea = FindViewById(EditDuration);
        }

        private void setupBindings() 
        {
            this.Bind(startTimeArea.Tapped(), _ => ViewModel.SelectTimeCommand.Execute(StartTime));
            this.Bind(stopTimeArea.Tapped(), _ => ViewModel.SelectTimeCommand.Execute(StopTime));
            this.Bind(durationArea.Tapped(), _ => ViewModel.SelectTimeCommand.Execute(Duration));
        }
    }
}
