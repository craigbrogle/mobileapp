using System;

namespace Toggl.Ultrawave.Serialization
{
    public class DeserializationException : Exception
    {
        private const string defaultMessage = "Something went wrong deserializing '{0}'.";

        public DeserializationException(Type modelType, Exception innerException = null)
            : base(string.Format(defaultMessage, modelType.Name), innerException)
        {
        }
    }
}
