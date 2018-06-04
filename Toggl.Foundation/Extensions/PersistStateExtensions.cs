using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Sync.States.Pull;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Extensions
{
    public static class PersistStateExtensions
    {
        internal static ApiExceptionsCatchingPersistState CatchApiExceptions(this IPersistState state)
            => new ApiExceptionsCatchingPersistState(state);

        internal static IPersistState UpdateSince<TInterface, TDatabaseInterface>(this IPersistState state, ISinceParameterRepository sinceParameterRepository)
            where TInterface : ILastChangedDatable
            where TDatabaseInterface : TInterface, IDatabaseSyncable
            => new SinceDateUpdatingPersistState<TInterface, TDatabaseInterface>(sinceParameterRepository, state);
    }
}
