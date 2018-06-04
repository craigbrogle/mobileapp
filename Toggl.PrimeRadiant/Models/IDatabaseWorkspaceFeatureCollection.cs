using System.Collections.Generic;
using Toggl.Multivac.Models;

namespace Toggl.PrimeRadiant.Models
{
    public interface IDatabaseWorkspaceFeatureCollection : IWorkspaceFeatureCollection, IDatabaseModel
    {
        IDatabaseWorkspace Workspace { get; }

        IEnumerable<IDatabaseWorkspaceFeature> DatabaseFeatures { get; }
    }
}
