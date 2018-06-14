using System;
using Foundation;
using Toggl.Foundation.Helper;
using MvvmCross.Plugins.Color.iOS;
using UIKit;
using Color = Toggl.Foundation.MvvmCross.Helper.Color;

namespace Toggl.Daneel.Views.EditDuration
{
    public sealed class DurationFieldTextFormatter
    {
        private const char splitDelimiter = ':';
        private static UIColor placeHolderColor = Color.Common.PlaceholderText.ToNativeColor();

        public static NSAttributedString AttributedStringFor(string durationText)
        {
            var result = new NSMutableAttributedString();
            var segments = durationText.Split(splitDelimiter);
            var isZeroChecking = true;

            for (int i = 0; i < segments.Length; ++i)
            {
                bool isLastSegment = i == segments.Length - 1;
                if ((segments[i] == "00" || segments[i] == "0") && isZeroChecking)
                {
                    result.Append(new NSAttributedString(segments[i], foregroundColor: placeHolderColor));
                    if (!isLastSegment)
                    {
                        result.Append(new NSAttributedString(splitDelimiter.ToString(), foregroundColor: placeHolderColor));
                    }
                }
                else
                {
                    isZeroChecking = false;
                    result.Append(new NSAttributedString(segments[i]));
                    if (!isLastSegment)
                    {
                        result.Append(new NSAttributedString(splitDelimiter.ToString()));
                    }
                }
            }
            return result;
        }
    }
}
