using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using static System.Reflection.BindingFlags;

namespace Toggl.Foundation.Analytics
{
    public static class AnalyticsEventCache
    {
        private static readonly Type displayNameType = typeof(DisplayNameAttribute);

        private static readonly object cacheAccess = new object();

        private static readonly Dictionary<string, TrackableEventTransformation> cache = new Dictionary<string, TrackableEventTransformation>();

        public static void TrackWith(this ITrackableEvent trackableEvent, IAnalyticsService service)
        {
            var eventType = trackableEvent.GetType();
            var typeName = eventType.Name;
            var transformator = (TrackableEventTransformation)null;

            lock (cacheAccess)
            {
                if (!cache.ContainsKey(typeName))
                    cache[typeName] = new TrackableEventTransformation(trackableEvent);

                transformator = cache[typeName];
            }

            var parameters = new Dictionary<string, string>();
            var eventName = eventType.NameOrDisplayName();

            if (transformator.IsValid)
            {
                parameters = transformator.Transform(trackableEvent, parameters);
                eventName = transformator.EventName;
            }
            else
            {
                parameters = convertObjectToDictionary(trackableEvent);
            }

            service.Track(eventName, parameters);
        }

        private static Dictionary<string, string> convertObjectToDictionary(ITrackableEvent obj) 
            => obj.GetType()
                  .GetProperties(Public | Instance)
                  .ToDictionary(
                      prop => prop.NameOrDisplayName(),
                      prop => prop.GetValue(obj).ToString());

        public static string NameOrDisplayName(this MemberInfo member)
        {
            var attr = member
                .GetCustomAttributes(displayNameType)
                .FirstOrDefault() as DisplayNameAttribute;

            return attr?.DisplayName ?? member.Name;
        }
    }
}
