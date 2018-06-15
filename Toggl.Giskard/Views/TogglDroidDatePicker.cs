using System;
using System.Linq;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using MvvmCross.Platform.Droid.Platform;
using Toggl.Giskard.Extensions;
using Toggl.Multivac;

namespace Toggl.Giskard.Views
{
    [Register("toggl.giskard.views.TogglDroidDatePicker")]
    public class TogglDroidDatePicker
        : DatePicker
        , DatePicker.IOnDateChangedListener
    {
        private bool isInitialized;

        private readonly long defaultMinimum =
            new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero)
            .ToUnixTimeMilliseconds();

        private readonly long defaultMaximum =
            new DateTimeOffset(2100, 1, 1, 0, 0, 0, TimeSpan.Zero)
            .ToUnixTimeMilliseconds();

        public TogglDroidDatePicker(Context context)
            : base(context)
        {
        }

        public TogglDroidDatePicker(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        protected TogglDroidDatePicker(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public event EventHandler BoundariesChanged;
        public event EventHandler ValueChanged;

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();

            var headerId = Resources.GetIdentifier("date_picker_header", "id", "android");
            var header = FindViewById(headerId);
            header.Visibility = ViewStates.Gone;
        }

        private DateTimeOffsetRange boundaries;
        public DateTimeOffsetRange Boundaries
        {
            get => boundaries;
            set
            {
                if (boundaries == value)
                    return;

                boundaries = value;

                /* 
                 * Workaround for a DatePicker bug in which
                 * there's an early return if the year is the same
                 * and the dates are different, which is bad logic.
                 * https://stackoverflow.com/a/19722636/93770
                 * 
                 * Also, because of the bug in DatePicker widget, make 
                 * sure this order is not reversed. MaxDate must be
                 * set before MinDate.
                 */

                MaxDate = defaultMaximum;
                MaxDate = ((DateTimeOffset)boundaries.Maximum.Date).ToUnixTimeMilliseconds();

                MinDate = defaultMinimum;
                MinDate = ((DateTimeOffset)boundaries.Minimum.Date).ToUnixTimeMilliseconds();

                BoundariesChanged?.Invoke(this, null);
            }
        }

        public DateTimeOffset originalValue;
        public DateTimeOffset Value
        {
            get => originalValue;
            set
            {
                if (originalValue == value)
                    return;
                
                originalValue = value;
                setDate(value.ToLocalTime());
            }
        }

        private void setDate(DateTimeOffset localTime)
        {
            var javaYear = localTime.Year;
            var javaMonth = localTime.Month - 1;
            var javaDay = localTime.Day;
            
            if (!isInitialized)
            {
                Init(javaYear, javaMonth, javaDay, this);
                isInitialized = true;
            }
            else if (Year != javaYear || Month != javaMonth || DayOfMonth != javaDay)
            {
                UpdateDate(javaYear, javaMonth, javaDay);
            }
        }

        public void OnDateChanged(DatePicker view, int year, int monthOfYear, int dayOfMonth)
        {
            var localTime = originalValue.ToLocalTime();

            var changedDateTimeOffset = new DateTimeOffset(
                year, monthOfYear + 1, dayOfMonth,
                localTime.Hour, localTime.Minute, localTime.Second, localTime.Offset);

            originalValue = changedDateTimeOffset;

            ValueChanged?.Invoke(this, null);
        }
    }
}