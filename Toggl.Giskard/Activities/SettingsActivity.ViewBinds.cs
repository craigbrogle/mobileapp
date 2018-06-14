using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace Toggl.Giskard.Activities
{
    public partial class SettingsActivity
    {
        private View manualModeView;
        private View avatarContainer;
        private View beginningOfWeekView;

        private TextView nameTextView;
        private TextView emailTextView;
        private TextView beginningOfWeekTextView;

        private ImageView avatarView;

        private Button helpButton;
        private Button logoutButton;
        private Button feedbackButton;

        private Switch manualModeSwitch;

        private RecyclerView workspacesRecyclerView;

        protected override void InitializeViews()
        {
            manualModeView = FindViewById(Resource.Id.SettingsToggleManualModeView);
            avatarContainer = FindViewById(Resource.Id.SettingsViewAvatarImageContainer);
            beginningOfWeekView = FindViewById(Resource.Id.SettingsSelectBeginningOfWeekView);

            nameTextView = FindViewById<TextView>(Resource.Id.SettingsNameTextView);
            emailTextView = FindViewById<TextView>(Resource.Id.SettingsEmailTextView);
            beginningOfWeekTextView = FindViewById<TextView>(Resource.Id.SettingsBeginningOfWeekTextView);

            helpButton = FindViewById<Button>(Resource.Id.SettingsHelpButton);
            logoutButton = FindViewById<Button>(Resource.Id.SettingsLogoutButton);
            feedbackButton = FindViewById<Button>(Resource.Id.SettingsSubmitFeedbackButton);

            avatarView = FindViewById<ImageView>(Resource.Id.SettingsViewAvatarImage);
            manualModeSwitch = FindViewById<Switch>(Resource.Id.SettingsIsManualModeEnabledSwitch);

            workspacesRecyclerView = FindViewById<RecyclerView>(Resource.Id.SettingsWorkspacesRecyclerView);
        }
    }
}
