using Newtonsoft.Json;

namespace Toggl.Foundation.Serialization
{
    public sealed class JsonSerializer : IJsonSerializer
    {
        public string Serialize(object toSerialize)
            => JsonConvert.SerializeObject(toSerialize);

        public bool TrySerialize(object toSerialize, out string result)
        {
            try {
                result = Serialize(toSerialize);
                return true;
            } catch (JsonSerializationException) {
                result = default(string);
                return false;
            }
        }

        public T Deserialize<T>(string json)
            => JsonConvert.DeserializeObject<T>(json);

        public bool TryDeserialize<T>(string json, out T result)
        {
            try {
                result = Deserialize<T>(json);
                return true;
            } catch (JsonSerializationException) {
                result = default(T);
                return false;
            }
        }
    }
}
