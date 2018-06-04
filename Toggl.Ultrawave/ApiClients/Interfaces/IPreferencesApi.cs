using System;
using Toggl.Multivac.Models;

namespace Toggl.Ultrawave.ApiClients
{
    public interface IPreferencesApi
        : IUpdatingApiClient<IPreferences>,
          IPullingSingleApiClient<IPreferences>
    {
    }
}
