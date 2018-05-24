
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using PropertyChanged;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels.Calendar;
using Toggl.Foundation.MvvmCross.ViewModels.Calendar.QuickSelectShortcuts;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class ReportsCalendarViewModel : MvxViewModel
    {
        private const int monthsToShow = 12;

        //Fields
        private readonly ITimeService timeService;
        private readonly ITogglDataSource dataSource;
        private readonly ISubject<DateRangeParameter> selectedDateRangeSubject = new Subject<DateRangeParameter>();
        private readonly string[] dayHeaders =
        {
            Resources.SundayInitial,
            Resources.MondayInitial,
            Resources.TuesdayInitial,
            Resources.WednesdayInitial,
            Resources.ThursdayInitial,
            Resources.FridayInitial,
            Resources.SaturdayInitial
        };

        private CalendarMonth initialMonth;
        private CalendarDayViewModel startOfSelection;

        private CompositeDisposable disposableBag;


        public BeginningOfWeek BeginningOfWeek { get; private set; }

        //Properties
        [DependsOn(nameof(CurrentPage))]
        public CalendarMonth CurrentMonth => convertPageIndexTocalendarMonth(CurrentPage);

        public int CurrentPage { get; set; } = monthsToShow - 1;

        [DependsOn(nameof(Months), nameof(CurrentPage))]
        public int RowsInCurrentMonth => Months[CurrentPage].RowCount;

        public List<CalendarPageViewModel> Months { get; } = new List<CalendarPageViewModel>();

        public IObservable<DateRangeParameter> SelectedDateRangeObservable
            => selectedDateRangeSubject.AsObservable();

        public List<CalendarBaseQuickSelectShortcut> QuickSelectShortcuts { get; private set; }

        public IMvxCommand<CalendarDayViewModel> CalendarDayTappedCommand { get; }

        public IMvxCommand<CalendarBaseQuickSelectShortcut> QuickSelectCommand { get; }

        public ReportsCalendarViewModel(
            ITimeService timeService, ITogglDataSource dataSource)
        {
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.timeService = timeService;
            this.dataSource = dataSource;

            CalendarDayTappedCommand = new MvxCommand<CalendarDayViewModel>(calendarDayTapped);
            QuickSelectCommand = new MvxCommand<CalendarBaseQuickSelectShortcut>(quickSelect);

            disposableBag = new CompositeDisposable();
        }

        private void calendarDayTapped(CalendarDayViewModel tappedDay)
        {
            if (startOfSelection == null)
            {
                var date = tappedDay.DateTimeOffset;

                var dateRange = DateRangeParameter.WithDates(date, date);
                startOfSelection = tappedDay;
                highlightDateRange(dateRange);
            }
            else
            {
                var startDate = startOfSelection.DateTimeOffset;
                var endDate = tappedDay.DateTimeOffset;

                var dateRange = DateRangeParameter.WithDates(startDate, endDate);
                startOfSelection = null;
                changeDateRange(dateRange);
            }
        }

        public override void Prepare()
        {
            base.Prepare();

            var now = timeService.CurrentDateTime;
            initialMonth = new CalendarMonth(now.Year, now.Month).AddMonths(-(monthsToShow - 1));
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            BeginningOfWeek = (await dataSource.User.Current).BeginningOfWeek;
            fillMonthArray();
            RaisePropertyChanged(nameof(CurrentMonth));

            QuickSelectShortcuts = createQuickSelectShortcuts();

            QuickSelectShortcuts
                .Select(quickSelectShortcut => SelectedDateRangeObservable.Subscribe(
                    quickSelectShortcut.OnDateRangeChanged))
                .ForEach(disposableBag.Add);

            quickSelect(QuickSelectShortcuts.Single(shortcut => shortcut is CalendarThisWeekQuickSelectShortcut));
        }

        public void OnToggleCalendar() => selectStartOfSelectionIfNeeded();

        public void OnHideCalendar() => selectStartOfSelectionIfNeeded();

        public string DayHeaderFor(int index)
            => dayHeaders[(index + (int)BeginningOfWeek + 7) % 7];

        private void selectStartOfSelectionIfNeeded()
        {
            if (startOfSelection == null) return;

            var date = startOfSelection.DateTimeOffset;
            var dateRange = DateRangeParameter.WithDates(date, date);
            changeDateRange(dateRange);
        }

        private void fillMonthArray()
        {
            var monthIterator = initialMonth;
            for (int i = 0; i < 12; i++, monthIterator = monthIterator.Next())
                Months.Add(new CalendarPageViewModel(monthIterator, BeginningOfWeek, timeService.CurrentDateTime));
        }

        private List<CalendarBaseQuickSelectShortcut> createQuickSelectShortcuts()
            => new List<CalendarBaseQuickSelectShortcut>
            {
                new CalendarTodayQuickSelectShortcut(timeService),
                new CalendarYesterdayQuickSelectShortcut(timeService),
                new CalendarThisWeekQuickSelectShortcut(timeService, BeginningOfWeek),
                new CalendarLastWeekQuickSelectShortcut(timeService, BeginningOfWeek),
                new CalendarThisMonthQuickSelectShortcut(timeService),
                new CalendarLastMonthQuickSelectShortcut(timeService),
                new CalendarThisYearQuickSelectShortcut(timeService)
            };

        private CalendarMonth convertPageIndexTocalendarMonth(int pageIndex)
            => initialMonth.AddMonths(pageIndex);

        private void changeDateRange(DateRangeParameter newDateRange)
        {
            startOfSelection = null;

            highlightDateRange(newDateRange);

            selectedDateRangeSubject.OnNext(newDateRange);
        }

        private void quickSelect(CalendarBaseQuickSelectShortcut quickSelectShortCut)
            => changeDateRange(quickSelectShortCut.GetDateRange());

        private void highlightDateRange(DateRangeParameter dateRange)
        {
            Months.ForEach(month => month.Days.ForEach(day => day.OnSelectedRangeChanged(dateRange)));
        }
    }
}
