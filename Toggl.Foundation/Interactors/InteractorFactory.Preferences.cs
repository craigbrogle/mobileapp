using System;
using Toggl.Foundation.Models.Interfaces;

namespace Toggl.Foundation.Interactors
{
    public sealed partial class InteractorFactory : IInteractorFactory
    {
        public IInteractor<IObservable<IThreadSafePreferences>> GetPreferences()
            => new GetPreferencesInteractor(dataSource);
    }
}
