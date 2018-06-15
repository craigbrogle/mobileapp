using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac;
using Xunit;

namespace Toggl.Foundation.Tests.Interactors
{
    public class GetPreferencesInteractorTests
    {
        public sealed class GetPreferencesInteractor : BaseInteractorTests
        {
            [Fact]
            public async Task CorrectPreferencesAreRetrieved()
            {
                IThreadSafePreferences prefs = Substitute.For<IThreadSafePreferences>();
                DataSource.Preferences.Current.Returns(Observable.Return(prefs));
             
                var result = await InteractorFactory.GetPreferences().Execute().FirstAsync();

                result.Should().Be(prefs);
            }
        }
    }
}
