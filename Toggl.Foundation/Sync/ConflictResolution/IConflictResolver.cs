using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Sync.ConflictResolution
{
    internal interface IConflictResolver<T>
    {
        ConflictResolutionMode Resolve(T localEntity, T serverEntity);
    }
}
