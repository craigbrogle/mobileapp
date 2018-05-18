using System;
namespace Toggl.Multivac.Models
{
    public interface ILocation
    {
        string City { get; }
        string State { get; }
        string CountryName { get; }
        string CountryCode { get; }
    }
}
