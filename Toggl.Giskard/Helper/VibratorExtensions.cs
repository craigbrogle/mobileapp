using System;
using Android.Media;
using Android.OS;

namespace Toggl.Giskard.Helper
{
    internal static class VibratorExtensions
    {
        public static void ActivateVibration(this Vibrator vibrator, int duration)
        {
            if (!vibrator.HasVibrator)
                return;

            try
            {
                if (OreoApis.AreAvailable)
                    vibrator.Vibrate(VibrationEffect.CreateOneShot(duration, 10));
                else
                    vibrator.Vibrate(duration);
            }
            catch
            {
                // Ignore potential permission exceptions
            }
        }
    }
}
