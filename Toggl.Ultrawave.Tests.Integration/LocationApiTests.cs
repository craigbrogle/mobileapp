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
            public async Task ReturnsNonEmptyLocation()
            {
                var api = TogglApiWith(Credentials.None);

                var location = await api.Location.Get();

                location.City.Should().NotBeNullOrEmpty();
                location.State.Should().NotBeNullOrEmpty();
                location.CountryName.Should().NotBeNullOrEmpty();
                location.CountryCode.Should().NotBeNullOrEmpty();
            }
        }
    }
}
