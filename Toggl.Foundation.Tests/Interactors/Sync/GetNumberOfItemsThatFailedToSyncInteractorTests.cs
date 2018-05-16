using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Tests.Mocks;
using Xunit;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Tests.Interactors
{
    public class GetNumberOfItemsThatFailedToSyncInteractorTests
    {
        public sealed class TheGetNumberOfItemsThatFailedToSyncInteractor : BaseInteractorTests
        {
            [Fact, LogIfTooSlow]
            public async Task ReturnsOnlyTagsThatFailedToSync()
            {
                MockClient[] clients = {
                    new MockClient { Id = 0, SyncStatus = SyncStatus.SyncFailed },
                    new MockClient { Id = 1, SyncStatus = SyncStatus.InSync },
                    new MockClient { Id = 2, SyncStatus = SyncStatus.SyncFailed },
                    new MockClient { Id = 3, SyncStatus = SyncStatus.SyncNeeded },
                    new MockClient { Id = 4, SyncStatus = SyncStatus.InSync },
                    new MockClient { Id = 5, SyncStatus = SyncStatus.SyncFailed }
                };

                Database.Clients.GetAll().Returns(Observable.Return(clients));

                MockProject[] projects = {
                    new MockProject { Id = 0, SyncStatus = SyncStatus.SyncFailed },
                    new MockProject { Id = 1, SyncStatus = SyncStatus.InSync },
                    new MockProject { Id = 2, SyncStatus = SyncStatus.SyncFailed },
                };

                Database.Projects.GetAll().Returns(Observable.Return(projects));

                MockTag[] tags = {
                };

                Database.Tags.GetAll().Returns(Observable.Return(tags));

                int syncFailedCount = clients.Where(p => p.SyncStatus == SyncStatus.SyncFailed).Count() +
                    projects.Where(p => p.SyncStatus == SyncStatus.SyncFailed).Count() +
                    tags.Where(p => p.SyncStatus == SyncStatus.SyncFailed).Count();


                int numberOfItems = await InteractorFactory.GetNumberOfItemsThatFailedToSync().Execute();

                numberOfItems.Should().Be(syncFailedCount);
            }
        }
    }
}
