using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Tests.Mocks;
using Xunit;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Tests.Interactors
{
    public class GetItemsThatFailedToSyncInteractorTests
    {
        public sealed class TheGetClientsThatFailedToSyncInteractor : BaseInteractorTests
        {
            [Fact, LogIfTooSlow]
            public async Task ReturnsOnlyItemsThatFailedToSync()
            {
                MockClient[] clients = {
                    new MockClient { Id = 0, SyncStatus = SyncStatus.SyncFailed },
                    new MockClient { Id = 1, SyncStatus = SyncStatus.InSync },
                    new MockClient { Id = 2, SyncStatus = SyncStatus.SyncFailed },
                    new MockClient { Id = 3, SyncStatus = SyncStatus.SyncNeeded },
                    new MockClient { Id = 4, SyncStatus = SyncStatus.InSync },
                    new MockClient { Id = 5, SyncStatus = SyncStatus.SyncFailed }
                };

                DataSource.Clients
                    .GetAll(Arg.Any<Func<IDatabaseClient, bool>>())
                    .Returns(callInfo =>
                    {
                        var filteredClients = clients.Where(callInfo.Arg<Func<IDatabaseClient, bool>>());
                        return Observable.Return(filteredClients.Cast<IThreadSafeClient>());
                    });

                MockProject[] projects = {
                    new MockProject { Id = 0, SyncStatus = SyncStatus.SyncFailed },
                    new MockProject { Id = 1, SyncStatus = SyncStatus.InSync },
                    new MockProject { Id = 2, SyncStatus = SyncStatus.SyncFailed },
                };

                DataSource.Projects
                    .GetAll(Arg.Any<Func<IDatabaseProject, bool>>())
                    .Returns(callInfo =>
                    {
                        var filteredProjects = projects.Where(callInfo.Arg<Func<IDatabaseProject, bool>>());
                        return Observable.Return(filteredProjects.Cast<IThreadSafeProject>());
                    });

                MockTag[] tags = {
                };

                DataSource.Tags
                    .GetAll(Arg.Any<Func<IDatabaseTag, bool>>())
                    .Returns(callInfo =>
                    {
                        var fiteredTags = tags.Where(callInfo.Arg<Func<IDatabaseTag, bool>>());
                        return Observable.Return(fiteredTags.Cast<IThreadSafeTag>());
                    });

                int syncFailedCount = clients.Count(p => p.SyncStatus == SyncStatus.SyncFailed) +
                                      projects.Count(p => p.SyncStatus == SyncStatus.SyncFailed) +
                                      tags.Count(p => p.SyncStatus == SyncStatus.SyncFailed);

                var returnedClients = await InteractorFactory.GetItemsThatFailedToSync().Execute();

                returnedClients.Count().Should().Be(syncFailedCount);
            }
        }
    }
}
