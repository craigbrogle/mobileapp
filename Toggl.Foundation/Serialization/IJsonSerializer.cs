using System;

namespace Toggl.Foundation.Serialization
{
    public interface IJsonSerializer
    {
        string Serialize(object toSerialize);

        bool TrySerialize(object toSerialize, out string result);

        T Deserialize<T>(string json);

        bool TryDeserialize<T>(string json, out T result);
    }
}
