using Toggl.PrimeRadiant;
using Toggl.Multivac;
using static Toggl.PrimeRadiant.ConflictResolutionMode;
using System;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Sync.ConflictResolution
{
    internal sealed class PreferNewer<T> : IConflictResolver<T>
        where T : class, ILastChangedDatable, IDatabaseSyncable
    {
        public TimeSpan MarginOfError { get; }

        public PreferNewer()
            : this(TimeSpan.Zero)
        {
        }

        public PreferNewer(TimeSpan marginOfError)
        {
            Ensure.Argument.IsNotNull(marginOfError, nameof(marginOfError));

            MarginOfError = marginOfError;
        }

        public ConflictResolutionMode Resolve(T localEntity, T serverEntity)
        {
            Ensure.Argument.IsNotNull(serverEntity, nameof(serverEntity));

            if (serverEntity is IDeletable deletable && deletable.ServerDeletedAt.HasValue)
                return localEntity == null ? Ignore : Delete;

            if (localEntity == null)
                return Create;

            if (localEntity.SyncStatus == SyncStatus.InSync)
                return Update;

            var receivedDataIsOutdated = localEntity.At.Add(MarginOfError) > serverEntity.At;
            if (receivedDataIsOutdated)
                return Ignore;

            return Update;
        }
    }
}
