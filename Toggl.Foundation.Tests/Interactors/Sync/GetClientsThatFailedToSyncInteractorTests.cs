using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Tests.Mocks;
using Xunit;
using Toggl.PrimeRadiant;
using FilterPredicate = System.Func<Toggl.PrimeRadiant.Models.IDatabaseClient, bool>;


namespace Toggl.Foundation.Tests.Interactors
{
    public class GetClientsThatFailedToSyncInteractorTests
    {
        public sealed class TheGetClientsThatFailedToSyncInteractor : BaseInteractorTests
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


                int syncFailedCount = clients.Where(p => p.SyncStatus == SyncStatus.SyncFailed).Count();

                Database.Clients
                    .GetAll(Arg.Any<FilterPredicate>())
                    .Returns(callInfo => Observable.Return(clients.Where(callInfo.Arg<FilterPredicate>())));

                var returnedClients = await InteractorFactory.GetClientsThatFailedToSync().Execute();

                returnedClients.Count().Should().Be(syncFailedCount);
            }
        }
    }
}
