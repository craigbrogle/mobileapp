using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.Multivac.Extensions;
using static Toggl.Multivac.Extensions.StringExtensions;
using Toggl.Foundation.Interactors;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectCountryViewModel : MvxViewModel<long?, long?>
    {
        private readonly IMvxNavigationService navigationService;

        private List<ICountry> allCountries;
        private long? selectedCountryId;

        public IMvxAsyncCommand CloseCommand { get; }

        public IMvxAsyncCommand<SelectableCountryViewModel> SelectCountryCommand { get; }

        public MvxObservableCollection<SelectableCountryViewModel> Suggestions { get; }
            = new MvxObservableCollection<SelectableCountryViewModel>();

        public string Text { get; set; } = "";

        public SelectCountryViewModel(IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.navigationService = navigationService;

            CloseCommand = new MvxAsyncCommand(close);
            SelectCountryCommand = new MvxAsyncCommand<SelectableCountryViewModel>(selectCountry);
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            allCountries = await new GetAllCountriesInteractor().Execute();
         
            var selectedElement = allCountries.Find(c => c.Id == selectedCountryId);
            if (selectedElement != null)
            {
                allCountries.Remove(selectedElement);
                allCountries.Insert(0, selectedElement);
            }

            Suggestions.AddRange(
                allCountries.Select(country => new SelectableCountryViewModel(
                    country,
                    country.Id == selectedCountryId)));
        }

        public override void Prepare(long? parameter)
        {
            selectedCountryId = parameter;
        }

        private void OnTextChanged()
        {
            Suggestions.Clear();
            var text = Text.Trim();
            Suggestions.AddRange(
                allCountries
                    .Where(c => c.Name.ContainsIgnoringCase(text))
                    .Select(c => new SelectableCountryViewModel(c, c.Id == selectedCountryId))
            );
        }

        private Task close()
            => navigationService.Close(this, null);

        private async Task selectCountry(SelectableCountryViewModel selectedCountry)
            => await navigationService.Close(this, selectedCountry.Country.Id);
    }
}
