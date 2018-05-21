using System;
using System.IO;
using System.Collections.Generic;
using Toggl.Multivac.Models;
using System.Reactive.Linq;
using Toggl.Foundation.Serialization;
using System.Reflection;
using System.Linq;
using Toggl.Multivac;
using Newtonsoft.Json;


namespace Toggl.Foundation.Interactors
{
    public class GetAllCountriesInteractor : IInteractor<IObservable<List<ICountry>>>
    {
        [Preserve(AllMembers = true)]
        private sealed partial class Country : ICountry
        {
            public long Id { get; set; }

            public string Name { get; set; }

            [JsonProperty("country_code")]
            public string CountryCode { get; set; }

            public Country() { }

            public Country(ICountry entity)
            {
                Id = entity.Id;
                Name = entity.Name;
                CountryCode = entity.CountryCode;
            }
        }

        public IObservable<List<ICountry>> Execute()
        {
            string json = Resources.CountriesJson;

            var countries = new Serialization.JsonSerializer()
                                             .Deserialize<List<Country>>(json)
                                             .ToList<ICountry>();

            return Observable.Return(countries);
        }
    }
}
