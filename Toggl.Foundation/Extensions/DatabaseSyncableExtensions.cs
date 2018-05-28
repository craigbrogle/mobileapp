using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Extensions
{
    public static class DatabaseSyncableExtensions
    {
        public static bool IsLocalOnly(this IDatabaseSyncable syncable)
            => syncable is IIdentifiable identifiable && identifiable.Id < 0;
    }
}
