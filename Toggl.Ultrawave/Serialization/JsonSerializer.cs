using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Toggl.Multivac.Models;
using static Toggl.Ultrawave.Serialization.SerializationReason;

namespace Toggl.Ultrawave.Serialization
{
    internal sealed class JsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerSettings defaultSettings = SerializerSettings.For(new DefaultContractResolver());

        private JsonSerializerSettings postSettings(IWorkspaceFeatureCollection features)
            => SerializerSettings.For(new FilterPropertiesContractResolver(
                new List<IPropertiesFilter>
                {
                    new IgnoreAttributeFilter<IgnoreWhenPostingAttribute>(),
                    new RequiresFeatureAttributeFilter(features)
                }));

        public T Deserialize<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, defaultSettings);
            }
            catch (Exception e)
            {
                throw new DeserializationException(typeof(T), e);
            }
        }

        public string Serialize<T>(T data, SerializationReason reason = Default, IWorkspaceFeatureCollection features = null)
        {
            try
            {
                return JsonConvert.SerializeObject(data, Formatting.None, getSettings(reason, features));
            }
            catch (Exception e)
            {
                throw new SerializationException(typeof(T), e);
            }
        }

        private JsonSerializerSettings getSettings(SerializationReason reason, IWorkspaceFeatureCollection features)
        {
            switch (reason)
            {
                case Post:
                    return postSettings(features);
                default:
                    return defaultSettings;
            }
        }
    }
}
