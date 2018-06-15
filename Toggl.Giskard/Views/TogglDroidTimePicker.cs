using System;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using MvvmCross.Platform.Droid.Platform;
using Toggl.Giskard.Extensions;
using Toggl.Giskard.Helper;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using JavaBool = Java.Lang.Boolean;

namespace Toggl.Giskard.Views
{
    [Register("toggl.giskard.views.TogglDroidTimePicker")]
    public class TogglDroidTimePicker
        : TimePicker
        , TimePicker.IOnTimeChangedListener
    {
        private bool isInitialized;

        public TogglDroidTimePicker(Context context, bool is24HoursMode)
            : base(context)
        {
           SetIs24HourView(new JavaBool(is24HoursMode));
        }

        public TogglDroidTimePicker(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        protected TogglDroidTimePicker(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        private DateTimeOffset originalValue;
        public DateTimeOffset Value
        {
            get => originalValue;
            set
            {
                if (!isInitialized)
                {
                    SetOnTimeChangedListener(this);
                    isInitialized = true;
                }

                if (originalValue == value)
                    return;

                originalValue = value;

                var localTime = value.ToLocalTime();

                if (MarshmallowApis.AreAvailable)
                {
                    if (Hour != localTime.Hour)
                        Hour = localTime.Hour;

                    if (Minute != localTime.Minute)
                        Minute = localTime.Minute;
                }
                else
                {
                    #pragma warning disable 0618

                    if ((int)CurrentHour != localTime.Hour)
                        CurrentHour = (Integer)localTime.Hour;

                    if ((int)CurrentMinute != localTime.Minute)
                        CurrentMinute = (Integer)localTime.Minute;
                    
                    #pragma warning restore 0618
                }
            }
        }

        public event EventHandler ValueChanged;

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();

            if (OreoApis.AreAvailable)
            {
                var keyboardIconId = Resources.GetIdentifier("toggle_mode", "id", "android");
                FindViewById(keyboardIconId).Visibility = ViewStates.Gone;
            }
        }

        public void OnTimeChanged(TimePicker view, int hourOfDay, int minute)
        {
            var localTime = originalValue.ToLocalTime();

            var changedDateTimeOffset = new DateTimeOffset(
                localTime.Year, localTime.Month, localTime.Day,
                hourOfDay, minute, localTime.Second, localTime.Offset);

            originalValue = changedDateTimeOffset;

            ValueChanged?.Invoke(this, null);
        }
    }
}