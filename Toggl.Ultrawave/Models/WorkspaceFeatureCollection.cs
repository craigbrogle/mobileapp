using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.Ultrawave.Serialization.Converters;

namespace Toggl.Ultrawave.Models
{
    internal sealed partial class WorkspaceFeatureCollection : IWorkspaceFeatureCollection
    {
        public long WorkspaceId { get; set; }

        [JsonConverter(typeof(ConcreteListTypeConverter<WorkspaceFeature, IWorkspaceFeature>))]
        public IEnumerable<IWorkspaceFeature> Features { get; set; }
    }
}
