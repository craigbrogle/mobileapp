using System.Collections.Generic;

namespace Toggl.Multivac.Models
{
    public interface IWorkspaceFeatureCollection
    {
        long WorkspaceId { get; }

        IEnumerable<IWorkspaceFeature> Features { get; }
    }
}
