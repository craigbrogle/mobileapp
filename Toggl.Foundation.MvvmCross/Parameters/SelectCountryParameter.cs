using Toggl.Multivac.Models;

namespace Toggl.Foundation.MvvmCross.Parameters
{
    public sealed class SelectCountryParameter
    {
        public string SelectedCountryCode { get; set; }
        public string SelectedCountryName { get; set; }

        public static SelectCountryParameter With(string selectedCountryCode, string selectedCountryName)
            => new SelectCountryParameter
            {
                SelectedCountryCode = selectedCountryCode,
                SelectedCountryName = selectedCountryName
            };
    }
}
