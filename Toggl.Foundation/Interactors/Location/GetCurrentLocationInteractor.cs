using System;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Interactors.Location
{
    public sealed class GetCurrentLocationInteractor : IInteractor<IObservable<ILocation>>
    {
        private readonly ITogglApi api;

        public GetCurrentLocationInteractor(ITogglApi api)
        {
            Ensure.Argument.IsNotNull(api, nameof(api));

            this.api = api;
        }

        public IObservable<ILocation> Execute()
            => api.Location.Get();
    }
}
