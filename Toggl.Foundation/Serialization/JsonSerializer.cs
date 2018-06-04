using Newtonsoft.Json;

namespace Toggl.Foundation.Serialization
{
    internal sealed class JsonSerializer : IJsonSerializer
    {
        public T Deserialize<T>(string json)
            => JsonConvert.DeserializeObject<T>(json);
    }
}
