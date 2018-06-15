using System;
using System.Reactive.Linq;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public sealed class PreferencesDataSource
        : SingletonDataSource<IThreadSafePreferences, IDatabasePreferences>, IPreferencesSource
    {
        public PreferencesDataSource(ISingleObjectStorage<IDatabasePreferences> storage)
            : base(storage, Preferences.DefaultPreferences)
        {
        }

        public IObservable<IThreadSafePreferences> Update(EditPreferencesDTO dto)
            => Get()
                .Select(preferences => updatedPreferences(preferences, dto))
                .SelectMany(Update);

        protected override IThreadSafePreferences Convert(IDatabasePreferences entity)
            => Preferences.From(entity);

        protected override ConflictResolutionMode ResolveConflicts(IDatabasePreferences first, IDatabasePreferences second)
            => Resolver.ForPreferences.Resolve(first, second);

        private IThreadSafePreferences updatedPreferences(IThreadSafePreferences existing, EditPreferencesDTO dto)
            => existing.With(
                dateFormat: dto.DateFormat,
                durationFormat: dto.DurationFormat,
                timeOfDayFormat: dto.TimeOfDayFormat,
                collapseTimeEntries: dto.CollapseTimeEntries,
                syncStatus: SyncStatus.SyncNeeded
            );
    }
}
