using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Toggl.Giskard.Helper;

namespace Toggl.Giskard.Extensions
{
    public static class ActivityExtensions
    {
        private static readonly Color lollipopFallbackStatusBarColor = Color.ParseColor("#2C2C2C");

        public static void ChangeStatusBarColor(this Activity activity, Color color, bool useDarkIcons = false)
        {
            var window = activity.Window;
            window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            window.SetStatusBarColor(color);

            if (MarshmallowApis.AreNotAvailable)
            {
                if (color == Color.White && useDarkIcons)
                {
                    window.SetStatusBarColor(lollipopFallbackStatusBarColor);
                }
                return;
            }

            window.DecorView.SystemUiVisibility =
                (StatusBarVisibility)(useDarkIcons ? SystemUiFlags.LightStatusBar : SystemUiFlags.Visible);
        }

        public static (int widthPixels, int heightPixels, bool isLargeScreen) GetMetrics(this Activity activity, Context context = null)
        {
            const int largeScreenThreshold = 360;

            context = context ?? activity.ApplicationContext;

            var displayMetrics = new DisplayMetrics();
            activity.WindowManager.DefaultDisplay.GetMetrics(displayMetrics);

            var isLargeScreen = displayMetrics.WidthPixels > largeScreenThreshold.DpToPixels(context);

            return (displayMetrics.WidthPixels, displayMetrics.HeightPixels, isLargeScreen);
        }

        public static void SetDefaultDialogLayout(this Window window, Activity activity, Context context, int heightDp)
        {
            const int smallScreenWidth = 312;
            const int largeScreenMargins = 72;

            var (widthPixels, heightPixels, isLargeScreen) = activity.GetMetrics(context);

            var width = isLargeScreen
                ? widthPixels - largeScreenMargins.DpToPixels(context)
                : smallScreenWidth.DpToPixels(context);

            var height = heightDp >= 0
                ? heightDp.DpToPixels(context)
                : heightDp;

            window.SetLayout(width, height);
        }
    }
}
