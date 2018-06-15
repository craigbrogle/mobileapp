using System;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public interface IUserSource : ISingletonDataSource<IThreadSafeUser>
    {
        IObservable<IDatabaseUser> UpdateWorkspace(long workspaceId);

        IObservable<IDatabaseUser> Update(EditUserDTO dto);
    }
}