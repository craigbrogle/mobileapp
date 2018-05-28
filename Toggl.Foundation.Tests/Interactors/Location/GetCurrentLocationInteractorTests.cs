using System.Threading.Tasks;
using System.Reactive.Linq;
using NSubstitute;
using Toggl.Foundation.Interactors.Location;
using Toggl.Ultrawave;
using Xunit;
using Toggl.Multivac.Models;
using FluentAssertions;

namespace Toggl.Foundation.Tests.Interactors.Location
{
    public sealed class GetCurrentLocationInteractorTests
    {
        public sealed class TheExecuteMethod
        {
            [Fact, LogIfTooSlow]
            public async Task ReturnsTheLocationApiResponse()
            {
                var location = Substitute.For<ILocation>();
                var api = Substitute.For<ITogglApi>();
                api.Location.Get().Returns(Observable.Return(location));
                var interactor = new GetCurrentLocationInteractor(api);

                var returnedLocation = await interactor.Execute();

                returnedLocation.Should().BeSameAs(location);
            }
        }
    }
}
