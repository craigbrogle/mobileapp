using Java.Lang;

namespace Toggl.Giskard.Extensions
{
    public static class StringExtensions
    {
        public static ICharSequence AsCharSequence(this string text)
            => new Java.Lang.String(text);
    }
}
