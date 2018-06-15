using System;
using System.Collections.Generic;
using System.Reflection;
using Android.Content;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Support.V7.AppCompat;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels.Hints;
using Toggl.Giskard.Activities;

namespace Toggl.Giskard.Presenters
{
    public sealed class TogglPresenter : MvxAppCompatViewPresenter
    {
        private readonly HashSet<Type> clearBackStackTypes = new HashSet<Type>
        {
            typeof(MainViewModel),
            typeof(LoginViewModel),
            typeof(SignupViewModel),
            typeof(OnboardingViewModel),
            typeof(TokenResetViewModel),
            typeof(OutdatedAppViewModel)
        };

        public TogglPresenter(IEnumerable<Assembly> androidViewAssemblies)
            : base(androidViewAssemblies)
        {
        }

        protected override Intent CreateIntentForRequest(MvxViewModelRequest request)
        {
            var intent = base.CreateIntentForRequest(request);

            if (clearBackStackTypes.Contains(request.ViewModelType))
            {
                intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask);
            }

            return intent;
        }

        public override void ChangePresentation(MvxPresentationHint hint)
        {
            switch (hint)
            {
                case ToggleCalendarVisibilityHint calendarHint when CurrentActivity is ReportsActivity reportsActivity:
                    reportsActivity.ToggleCalendarState(calendarHint.ForceHide);
                    return;
            }

            base.ChangePresentation(hint);
        }
    }
}
