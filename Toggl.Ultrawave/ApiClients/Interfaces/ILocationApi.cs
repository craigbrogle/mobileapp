using System;
using Toggl.Multivac.Models;

namespace Toggl.Ultrawave.ApiClients.Interfaces
{
    public interface ILocationApi
    {
        IObservable<ILocation> Get();
    }
}
