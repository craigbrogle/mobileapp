using System;
using System.Globalization;
using MvvmCross.Platform.Converters;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Converters
{
    [Preserve(AllMembers = true)]
    public sealed class StripAmPmValueConverter : MvxValueConverter<string, string>
    {
        protected override string Convert(string value, Type targetType, object parameter, CultureInfo culture) 
            => value.Replace("AM", "").Replace("PM", "").Trim();
    }
}
