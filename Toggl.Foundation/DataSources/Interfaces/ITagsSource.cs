using System;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public interface ITagsSource : IDataSource<IThreadSafeTag, IDatabaseTag>
    {
        IObservable<IThreadSafeTag> Create(string name, long workspaceId);
    }
}