using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using Toggl.Multivac;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Models;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class SyncFailuresViewModel : MvxViewModel
    {
        public IEnumerable<SyncFailureItem> SyncFailures { get; private set; }

        private readonly IInteractorFactory interactorFactory;

        public SyncFailuresViewModel(IInteractorFactory interactorFactory)
        {
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));

            this.interactorFactory = interactorFactory;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            SyncFailures = await interactorFactory
                .GetItemsThatFailedToSync().Execute()
                .FirstAsync();
        }
    }
}
