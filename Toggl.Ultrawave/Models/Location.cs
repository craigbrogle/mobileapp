using System;
using System.Runtime.Serialization;
using Toggl.Multivac.Models;

namespace Toggl.Ultrawave.Models
{
    internal sealed partial class Location : ILocation
    {
        public string CountryName { get; set; }

        public string CountryCode { get; set; }

        [OnSerializing]
        internal void OnSerializing(StreamingContext context)
        {
            throw new InvalidOperationException($"The {nameof(Location)} model serialization is not supported.");
        }
    }
}
