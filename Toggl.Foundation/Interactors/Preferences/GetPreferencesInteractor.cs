using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models.Interfaces;
using static Toggl.Multivac.WorkspaceFeatureId;

namespace Toggl.Foundation.Interactors
{
    internal sealed class GetPreferencesInteractor : IInteractor<IObservable<IThreadSafePreferences>>
    {
        private readonly ITogglDataSource dataSource;

        public GetPreferencesInteractor(ITogglDataSource dataSource)
        {
            this.dataSource = dataSource;
        }

        public IObservable<IThreadSafePreferences> Execute() 
            => dataSource.Preferences.Current;
    }
}
