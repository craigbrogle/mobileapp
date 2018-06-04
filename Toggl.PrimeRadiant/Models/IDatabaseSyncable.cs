using Toggl.PrimeRadiant.Models;

namespace Toggl.PrimeRadiant
{
    public interface IDatabaseSyncable : IDatabaseModel
    {
        SyncStatus SyncStatus { get; }

        string LastSyncErrorMessage { get; }

        bool IsDeleted { get; }
    }
}
