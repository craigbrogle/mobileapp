using System;
using Newtonsoft.Json;
using Toggl.Multivac.Models;

namespace Toggl.Ultrawave.Models
{
    internal sealed partial class Location : ILocation
    {
        public string City { get; set; }

        public string State { get; set; }

        public string CountryName { get; set; }

        public string CountryCode { get; set; }
    }
}
