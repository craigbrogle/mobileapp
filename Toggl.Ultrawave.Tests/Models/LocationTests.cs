using FluentAssertions;
using Toggl.Ultrawave.Models;
using Xunit;

namespace Toggl.Ultrawave.Tests.Models
{
    public sealed class LocationTests
    {
        public sealed class TheLocationModel
        {
            private string validJson
                = "{\"city\":\"Riga\",\"state\":\"Riga\",\"country_name\":\"Latvia\",\"country_code\":\"LV\"}";

            private Location validLocation = new Location
            {
                City = "Riga",
                State = "Riga",
                CountryName = "Latvia",
                CountryCode = "LV"
            };

            [Fact, LogIfTooSlow]
            public void HasConstructorWhichCopiesValuesFromInterfaceToTheNewInstance()
            {
                var clonedObject = new Location(validLocation);

                clonedObject.Should().NotBeSameAs(validLocation);
                clonedObject.ShouldBeEquivalentTo(validLocation, options => options.IncludingProperties());
            }

            [Fact, LogIfTooSlow]
            public void CanBeDeserialized()
            {
                SerializationHelper.CanBeDeserialized(validJson, validLocation);
            }

            [Fact, LogIfTooSlow]
            public void CanBeSerialized()
            {
                SerializationHelper.CanBeSerialized(validJson, validLocation);
            }
        }
    }
}
