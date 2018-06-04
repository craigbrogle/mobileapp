using Toggl.Multivac.Models;

namespace Toggl.PrimeRadiant.Models
{
    public interface IDatabasePreferences : IPreferences, IDatabaseSyncable, IIdentifiable
    {
    }
}
