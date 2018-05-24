using System;
using System.Globalization;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Core;
using PropertyChanged;
using Toggl.Foundation.Helper;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;
using System.Reactive.Subjects;
using static Toggl.Foundation.MvvmCross.ViewModels.SelectTimeViewModel.TemporalInconsistency;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectTimeViewModel
        : MvxViewModel<SelectTimeParameters, SelectTimeResultsParameters>
    {
        public const int StartTimeTab = 0;
        public const int StopTimeTab = 1;
        public const int DurationTab = 2;

        public enum TemporalInconsistency
        {
            StartTimeAfterStopTime,
            StopTimeBeforeStartTime,
            DurationTooLong
        }

        private readonly ISubject<TemporalInconsistency> temporalInconsistencySubject = new Subject<TemporalInconsistency>();

        private readonly IMvxNavigationService navigationService;
        private readonly ITimeService timeService;
        private IDisposable timeServiceDisposable;
        private bool isViewModelPrepared;
        private TimeSpan? editingDuration;

        public DateTimeOffsetRange StartTimeBoundaries { get; set; }

        public DateTimeOffsetRange StopTimeBoundaries { get; set; }

        public DateTimeOffset CurrentDateTime { get; set; }

        public DateTimeOffset StartTime { get; set; }

        public int StartingTabIndex { get; private set; }

        public DateFormat DateFormat { get; set; }

        public TimeFormat TimeFormat { get; set; }

        private bool stopTimeEntryRequested;

        [DependsOn(nameof(CurrentDateTime))]
        public DateTimeOffset? StopTime { get; set; }

        public DateTimeOffset StopTimeOrCurrent => StopTime ?? CurrentDateTime;

        public IObservable<TemporalInconsistency> TemporalInconsistencyDetected { get; }

        [DependsOn(nameof(StartTime))]
        public DateTime StartDatePart
        {
            get => StartTime.Date;
            set
            {
                var newYear = value.Year;
                var newMonth = value.Month;
                var newDay = value.Day;

                if (StartTime.Year == newYear && StartTime.Month == newMonth && StartTime.Day == newDay)
                    return;

                StartTime = new DateTimeOffset(
                    newYear, newMonth, newDay,
                    StartTime.Hour, StartTime.Minute, StartTime.Second, StartTime.Offset);

                RaisePropertyChanged(nameof(StartDatePart));
            }
        }

        [DependsOn(nameof(StartTime))]
        public TimeSpan StartTimePart
        {
            get => StartTime.TimeOfDay;
            set
            {
                var newTimeHours = value.Hours;
                var newTimeMinutes = value.Minutes;

                if (StartTime.TimeOfDay.Hours == newTimeHours && StartTime.TimeOfDay.Minutes == newTimeMinutes)
                    return;

                StartTime = new DateTimeOffset(
                    StartTime.Year, StartTime.Month, StartTime.Day,
                    newTimeHours, newTimeMinutes, 0, StartTime.Offset);

                RaisePropertyChanged(nameof(StartTimePart));
            }
        }

        [DependsOn(nameof(StopTime))]
        public DateTime StopDatePart
        {
            get => StopTime?.Date ?? default(DateTime);
            set
            {
                if (!StopTime.HasValue && !stopTimeEntryRequested)
                    return;

                var startDateYear = StopTime?.Year;
                var startDateMonth = StopTime?.Month;
                var startDateDay = StopTime?.Day;

                var newYear = value.Year;
                var newMonth = value.Month;
                var newDay = value.Day;

                if (startDateYear == newYear && startDateMonth == newMonth && startDateDay == newDay)
                    return;

                var now = CurrentDateTime;
                var hour = StopTime?.Hour ?? now.Hour;
                var minute = StopTime?.Minute ?? now.Minute;
                var second = StopTime?.Second ?? now.Second;
                var offset = StopTime?.Offset ?? now.Offset;

                StopTime = new DateTimeOffset(
                    newYear, newMonth, newDay,
                    hour, minute, second, offset);

                RaisePropertyChanged(nameof(StopDatePart));
            }
        }

        [DependsOn(nameof(StopTime))]
        public TimeSpan StopTimePart
        {
            get => StopTime?.TimeOfDay ?? default(TimeSpan);
            set
            {
                if (!StopTime.HasValue && !stopTimeEntryRequested)
                    return;

                var stopTimeHours = StopTime?.TimeOfDay.Hours;
                var stopTimeMinutes = StopTime?.TimeOfDay.Minutes;

                var newTimeHours = value.Hours;
                var newTimeMinutes = value.Minutes;

                if (stopTimeHours == newTimeHours && stopTimeMinutes == newTimeMinutes)
                    return;

                var now = CurrentDateTime;
                var year = StopTime?.Year ?? now.Year;
                var month = StopTime?.Month ?? now.Month;
                var day = StopTime?.Day ?? now.Day;
                var offset = StopTime?.Offset ?? now.Offset;

                StopTime = new DateTimeOffset(
                    year, month, day,
                    newTimeHours, newTimeMinutes, 0, offset);

                RaisePropertyChanged(nameof(StopTimePart));
            }
        }

        public bool IsEditingDuration { get; set; }

        [DependsOn(nameof(StopTime))]
        public bool IsTimeEntryStopped => StopTime.HasValue;

        public IMvxCommand StopTimeEntryCommand { get; }

        public IMvxAsyncCommand CancelCommand { get; }

        public IMvxAsyncCommand SaveCommand { get; }

        public IMvxCommand ToggleClockCalendarModeCommand { get; }

        public IMvxCommand<int> IncreaseDurationCommand { get; }

        public IMvxCommand FocusDurationCommand { get; }

        public IMvxCommand UnfocusDurationCommand { get; }

        public bool IsCalendarView { get; set; }

        public int CurrentTab { get; set; }

        public bool IsOnStartTimeTab => CurrentTab == StartTimeTab;

        public bool IsOnStopTimeTab => CurrentTab == StopTimeTab;

        public bool IsOnDurationTab => CurrentTab == DurationTab;

        public bool IsDurationVisible { get; set; }

        [DependsOn(nameof(StartTime), nameof(StopTime))]
        public TimeSpan Duration
            => (StopTime ?? CurrentDateTime) - StartTime;

        public TimeSpan EditingDuration
        {
            get => editingDuration ?? Duration;
            set
            {
                if (StopTime.HasValue)
                {
                    StopTime = StartTime + value;
                }
                else
                {
                    StartTime = CurrentDateTime - value;
                }

                editingDuration = (StopTime ?? CurrentDateTime) - StartTime;
            }
        }

        [DependsOn(nameof(CurrentDateTime))]
        public bool IsRunningTimeEntryAM => CurrentDateTime.Hour < 12;

        public SelectTimeViewModel(
            IMvxNavigationService navigationService,
            ITimeService timeService)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));

            TemporalInconsistencyDetected = temporalInconsistencySubject.AsObservable();

            this.navigationService = navigationService;
            this.timeService = timeService;

            CancelCommand = new MvxAsyncCommand(cancel);
            SaveCommand = new MvxAsyncCommand(save);

            IncreaseDurationCommand = new MvxCommand<int>(increaseDuration);

            FocusDurationCommand = new MvxCommand(focusDurationCommand);
            UnfocusDurationCommand = new MvxCommand(unfocusDurationCommand);

            StopTimeEntryCommand = new MvxCommand(stopTimeEntry);

            ToggleClockCalendarModeCommand = new MvxCommand(togglClockCalendarMode);
        }

        public override void Prepare(SelectTimeParameters parameter)
        {
            StartTime = parameter.Start.ToLocalTime();
            StopTime = parameter.Stop?.ToLocalTime();

            DateFormat = parameter.DateFormat;
            TimeFormat = parameter.TimeFormat;

            StartingTabIndex = parameter.StartingTabIndex;
            IsCalendarView = parameter.ShouldStartOnCalendar;

            initializeTimeConstraints();
            isViewModelPrepared = true;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            if (!StopTime.HasValue)
            {
                timeServiceDisposable =
                    timeService.CurrentDateTimeObservable
                               .StartWith(timeService.CurrentDateTime)
                               .Subscribe(currentDateTime => CurrentDateTime = currentDateTime);
            }
        }

        private void increaseDuration(int minutes)
        {
            EditingDuration += TimeSpan.FromMinutes(minutes);
        }

        private void focusDurationCommand()
        {
            IsEditingDuration = true;
        }

        private void unfocusDurationCommand()
        {
            IsEditingDuration = false;
        }

        private void togglClockCalendarMode()
        {
            IsCalendarView = !IsCalendarView;
        }

        private void stopTimeEntry()
        {
            stopTimeEntryRequested = true;
            StopTime = CurrentDateTime;
        }

        private void initializeTimeConstraints()
        {
            StopTimeBoundaries = new DateTimeOffsetRange(
                StartTime,
                StartTime + Constants.MaxTimeEntryDuration);

            if (StopTime.HasValue)
            {
                StartTimeBoundaries = new DateTimeOffsetRange(
                    StopTime.Value - Constants.MaxTimeEntryDuration,
                    StopTime.Value);
            }
            else
            {
                StartTimeBoundaries = new DateTimeOffsetRange(
                    Constants.EarliestAllowedStartTime,
                    Constants.LatestAllowedStartTime);
            }
        }

        private void OnStartTimeChanged()
        {
            if (!isViewModelPrepared)
                return;

            if (StopTime.HasValue)
            {
                if (StopTime < StartTime)
                {
                    StartTime = StopTime.Value;
                    temporalInconsistencySubject.OnNext(StartTimeAfterStopTime);
                }

                if (StopTime.Value - StartTime > Constants.MaxTimeEntryDuration)
                {
                    StartTime = StopTime.Value - Constants.MaxTimeEntryDuration;
                    temporalInconsistencySubject.OnNext(DurationTooLong);
                }
            }

            StopTimeBoundaries = new DateTimeOffsetRange(
                StartTime,
                StartTime + Constants.MaxTimeEntryDuration);
        }

        private void OnStopTimeChanged()
        {
            if (!isViewModelPrepared)
                return;

            if (StopTime.HasValue)
            {
                if (StopTime < StartTime)
                {
                    StopTime = StartTime;
                    temporalInconsistencySubject.OnNext(StopTimeBeforeStartTime);
                }

                if (StopTime.Value - StartTime > Constants.MaxTimeEntryDuration)
                {
                    StopTime = StartTime + Constants.MaxTimeEntryDuration;
                    temporalInconsistencySubject.OnNext(DurationTooLong);
                }

                StartTimeBoundaries = new DateTimeOffsetRange(
                    StopTime.Value - Constants.MaxTimeEntryDuration,
                    StopTime.Value);
            }
            else
            {
                StartTimeBoundaries = new DateTimeOffsetRange(
                    Constants.EarliestAllowedStartTime,
                    Constants.LatestAllowedStartTime);
            }
        }

        public override void ViewDestroy()
        {
            base.ViewDestroy();
            timeServiceDisposable?.Dispose();
        }

        private async Task cancel()
            => await navigationService.Close(this, null);

        private async Task save()
        {
            var results = new SelectTimeResultsParameters(StartTime.ToUniversalTime(), StopTime?.ToUniversalTime());
            await navigationService.Close(this, results);
        }
    }
}
