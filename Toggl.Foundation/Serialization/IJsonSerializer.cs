using System;

namespace Toggl.Foundation.Serialization
{
    internal interface IJsonSerializer
    {
        T Deserialize<T>(string json);
    }
}
