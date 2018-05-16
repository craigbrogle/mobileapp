using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Tests.Mocks;
using Xunit;
using Toggl.PrimeRadiant;
using FilterPredicate = System.Func<Toggl.PrimeRadiant.Models.IDatabaseTag, bool>;

namespace Toggl.Foundation.Tests.Interactors
{
    public class GetTagsThatFailedToSyncInteractorTests
    {
        public sealed class TheGetTagsThatFailedToSyncInteractor : BaseInteractorTests
        {
            [Fact, LogIfTooSlow]
            public async Task ReturnsOnlyTagsThatFailedToSync()
            {
                MockTag[] tags = {
                    new MockTag { Id = 0, SyncStatus = SyncStatus.SyncFailed },
                    new MockTag { Id = 1, SyncStatus = SyncStatus.InSync },
                    new MockTag { Id = 2, SyncStatus = SyncStatus.SyncFailed },
                    new MockTag { Id = 3, SyncStatus = SyncStatus.SyncNeeded },
                    new MockTag { Id = 4, SyncStatus = SyncStatus.InSync },
                    new MockTag { Id = 5, SyncStatus = SyncStatus.SyncFailed }
                };

                int syncFailedCount = tags.Where(p => p.SyncStatus == SyncStatus.SyncFailed).Count();

                Database.Tags
                    .GetAll(Arg.Any<FilterPredicate>())
                    .Returns(callInfo => Observable.Return(tags.Where(callInfo.Arg<FilterPredicate>())));

                var returnedTags = await InteractorFactory.GetTagsThatFailedToSync().Execute();

                returnedTags.Count().Should().Be(syncFailedCount);
            }
        }
    }
}
