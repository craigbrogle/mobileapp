using System;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Models.Interfaces;

namespace Toggl.Foundation.DataSources
{
    public interface IPreferencesSource : ISingletonDataSource<IThreadSafePreferences>
    {
        IObservable<IThreadSafePreferences> Update(EditPreferencesDTO dto);
    }
}
