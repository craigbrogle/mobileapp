using System;
using System.Globalization;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using PropertyChanged;
using Toggl.Foundation.MvvmCross.Combiners;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectTimeViewModel
        : MvxViewModel<SelectTimeParameters, SelectTimeResultsParameters>
    {
        public const int StartTimeTab = 0;
        public const int StopTimeTab = 1;
        public const int DurationTab = 2;

        private readonly IMvxNavigationService navigationService;
        private readonly ITimeService timeService;
        private IDisposable timeServiceDisposable;

        private readonly TimeSpanToDurationValueConverter durationConverter = new TimeSpanToDurationValueConverter();

        private readonly DateTimeOffsetTimeFormatValueCombiner timeFormatValueCombiner;
        private readonly DateTimeOffsetDateFormatValueCombiner dateFormatValueCombiner;

        public DateTimeOffset CurrentDateTime { get; set; }

        public DateTimeOffset StartTime { get; set; }

        public int StartingTabIndex { get; private set; }

        public DateFormat DateFormat { get; set; }
        public TimeFormat TimeFormat { get; set; }

        [DependsOn(nameof(CurrentDateTime))]
        public DateTimeOffset? StopTime { get; set; }

        public DateTimeOffset StopTimeOrCurrent => StopTime ?? CurrentDateTime;

        [DependsOn(nameof(StartTime))]
        public DateTime StartDatePart
        {
            get
            {
                return StartTime.Date;
            }
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
            get
            {
                return StartTime.TimeOfDay;
            }
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
            get
            {
                return StopTime?.Date ?? default(DateTime);
            }
            set
            {
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
            get
            {
                return StopTime?.TimeOfDay ?? default(TimeSpan);
            }
            set
            {
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

        [DependsOn(nameof(Duration))]
        public string DurationText
            => (string)durationConverter.Convert(Duration, typeof(TimeSpan), null, CultureInfo.CurrentCulture);

        private TimeSpan? editingDuration = null;
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

            this.navigationService = navigationService;
            this.timeService = timeService;

            CancelCommand = new MvxAsyncCommand(cancel);
            SaveCommand = new MvxAsyncCommand(save);

            IncreaseDurationCommand = new MvxCommand<int>(increaseDuration);

            FocusDurationCommand = new MvxCommand(focusDurationCommand);
            UnfocusDurationCommand = new MvxCommand(unfocusDurationCommand);

            StopTimeEntryCommand = new MvxCommand(stopTimeEntry);

            ToggleClockCalendarModeCommand = new MvxCommand(togglClockCalendarMode);

            timeFormatValueCombiner = new DateTimeOffsetTimeFormatValueCombiner(TimeZoneInfo.Local);
            dateFormatValueCombiner = new DateTimeOffsetDateFormatValueCombiner(TimeZoneInfo.Local);
        }

        public override void Prepare(SelectTimeParameters parameter)
        {
            StartTime = parameter.Start.ToLocalTime();
            StopTime = parameter.Stop?.ToLocalTime();

            DateFormat = parameter.DateFormat;
            TimeFormat = parameter.TimeFormat;

            StartingTabIndex = parameter.StartingTabIndex;
            IsCalendarView = parameter.ShouldStartOnCalendar;
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
            StopTime = CurrentDateTime;
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
