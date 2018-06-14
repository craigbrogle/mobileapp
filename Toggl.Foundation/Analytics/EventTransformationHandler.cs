using System.Collections.Generic;

namespace Toggl.Foundation.Analytics
{
    internal delegate Dictionary<string, string> EventTransformationHandler(ITrackableEvent trackableEvent, Dictionary<string, string> parameters);
}