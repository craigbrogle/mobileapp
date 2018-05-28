using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Tests.Mocks;
using Xunit;

namespace Toggl.Foundation.Tests.Interactors
{
    public class GetAllWorkspacesInteractorTests
    {
        public sealed class TheGetAllWorkspacesInteractor : BaseInteractorTests
        {
            [Fact, LogIfTooSlow]
            public async Task ReturnsAllWorkspacesInTheDatabase()
            {
                var workspaces =
                    Enumerable.Range(0, 10)
                        .Select(id => new MockWorkspace { Id = id });

                DataSource.Workspaces.GetAll().Returns(Observable.Return(workspaces));

                var returnedWorkspaces = await InteractorFactory.GetAllWorkspaces().Execute();

                returnedWorkspaces.Count().Should().Be(workspaces.Count());
            }
        }
    }
}
