using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant.Models;
using Xunit;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class SelectCountryViewModelTests
    {
        public abstract class SelectCountryViewModelTest : BaseViewModelTests<SelectCountryViewModel>
        {
            protected SelectCountryParameter Parameters { get; }
                = SelectCountryParameter.With("", "");

            protected override SelectCountryViewModel CreateViewModel()
                => new SelectCountryViewModel(NavigationService);

            protected List<ICountry> GenerateCountriesList() =>
                Enumerable.Range(1, 10).Select(i =>
                {
                    var country = Substitute.For<ICountry>();
                    country.Id.Returns(i);
                    country.Name.Returns(i.ToString());
                    country.CountryCode.Returns(i.ToString());
                    return country;
                }).ToList();
        }

        public sealed class TheConstructor : SelectCountryViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void ThrowsIfTheArgumentIsNull()
            {
                Action tryingToConstructWithEmptyParameters =
                    () => new SelectCountryViewModel(null);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheInitializeMethod : SelectCountryViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task AddsAllCountriesToTheListOfSuggestions()
            {
                var countries = GenerateCountriesList();
                
                ViewModel.Prepare(Parameters);

                await ViewModel.Initialize();

                ViewModel.Suggestions.Should().HaveCount(250);
            }

            [Theory, LogIfTooSlow]
            [InlineData("AL")]
            [InlineData("GR")]
            [InlineData("EE")]
            public async Task SetsTheAppropriateCountryAsTheCurrentlySelectedOne(string code)
            {
                var parameter = SelectCountryParameter.With(code, "");

                ViewModel.Prepare(parameter);

                await ViewModel.Initialize();

                ViewModel.Suggestions.Single(c => c.Selected).Country.CountryCode.Should().Be(code);
            }
        }

        public sealed class TheSelectCountryCommand : SelectCountryViewModelTest
        {
            public TheSelectCountryCommand()
            {
                ViewModel.Prepare(Parameters);
            }

            [Fact, LogIfTooSlow]
            public async Task ClosesTheViewModelWithSelectedCountryCode()
            {
                var country = Substitute.For<ICountry>();
                country.Id.Returns(1);
                country.Name.Returns("Greece");
                country.CountryCode.Returns("GR");

                await ViewModel.Initialize();

                var selectableCountry = new SelectableCountryViewModel(country, true);

                ViewModel.SelectCountryCommand.Execute(selectableCountry);

                await NavigationService.Received()
                   .Close(Arg.Is(ViewModel), Arg.Any<SelectCountryParameter>());
            }
        }

        public sealed class TheTextProperty : SelectCountryViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task FiltersTheSuggestionsWhenItChanges()
            {
                ViewModel.Prepare(Parameters);
                await ViewModel.Initialize();

                ViewModel.Text = "Greece";

                ViewModel.Suggestions.Should().HaveCount(1);
            }
        }
    }
}
