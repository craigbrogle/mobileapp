using System;
using System.Globalization;
using Foundation;
using MvvmCross.Platform.Converters;
using Toggl.Multivac;
using UIKit;

namespace Toggl.Daneel.Converters
{
    public sealed class StringToAttributedStringValueConverter
        : MvxValueConverter<string, NSAttributedString>
    {
        private readonly UIStringAttributes attributes;

        public StringToAttributedStringValueConverter(UIStringAttributes attributes)
        {
            Ensure.Argument.IsNotNull(attributes, nameof(attributes));

            this.attributes = attributes;
        }

        protected override NSAttributedString Convert(string value, Type targetType, object parameter, CultureInfo culture)
            => new NSAttributedString(value, attributes);

        protected override string ConvertBack(NSAttributedString value, Type targetType, object parameter, CultureInfo culture)
            => value.Value;
    }
}
