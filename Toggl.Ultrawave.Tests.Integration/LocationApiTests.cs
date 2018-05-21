using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Toggl.Ultrawave.Network;
using Toggl.Ultrawave.Tests.Integration.BaseTests;
using Xunit;

namespace Toggl.Ultrawave.Tests.Integration
{
    public sealed class LocationApiTests
    {
        public sealed class TheGetMethod : EndpointTestBase
        {
            [Fact, LogIfTooSlow]
            public async Task ReturnsNonEmptyCountry()
            {
                var api = TogglApiWith(Credentials.None);

                var location = await api.Location.Get();

                location.CountryName.Should().NotBeNullOrEmpty();
                location.CountryCode.Should().NotBeNullOrEmpty();
            }
        }
    }
}
