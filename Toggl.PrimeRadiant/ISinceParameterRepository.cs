using System;

namespace Toggl.PrimeRadiant
{
    public interface ISinceParameterRepository
    {
        DateTimeOffset? Get<T>() where T : IDatabaseSyncable;

        void Set<T>(DateTimeOffset? since);

        bool Supports<T>();
    }
}
