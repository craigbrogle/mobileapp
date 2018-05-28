using System;

namespace Toggl.Multivac.Models
{
    public interface ICountry : IIdentifiable
    {
        string Name { get; }
        string CountryCode { get; }
    }
}
