using MvvmCross.Core.ViewModels;
using Toggl.Multivac;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectableCountryViewModel : MvxNotifyPropertyChanged
    {
        public ICountry Country { get; set; }

        public bool Selected { get; set; }

        public SelectableCountryViewModel(ICountry country, bool selected)
        {
            Country = country;
            Selected = selected;
        }

        public override string ToString() => Country.Name;
    }
}
