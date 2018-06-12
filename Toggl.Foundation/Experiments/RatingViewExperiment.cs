using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Services;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Foundation.Experiments
{
    [Preserve(AllMembers = true )]
    public sealed class RatingViewExperiment
    {
        private readonly ITimeService timeService;
        private readonly ITogglDataSource dataSource;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IRemoteConfigService remoteConfigService;

        public IObservable<bool> RatingViewShouldBeVisible
            => remoteConfigService
                .RatingViewConfiguration
                .SelectMany(criterionMatched)
                .Select(tuple => tuple.criterionMatched && dayCountPassed(tuple.configuration));

        public RatingViewExperiment(
            ITimeService timeService,
            ITogglDataSource dataSource,
            IOnboardingStorage onboardingStorage,
            IRemoteConfigService remoteConfigService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(remoteConfigService, nameof(remoteConfigService));

            this.dataSource = dataSource;
            this.timeService = timeService;
            this.onboardingStorage = onboardingStorage;
            this.remoteConfigService = remoteConfigService;
        }

        private bool dayCountPassed(RatingViewConfiguration ratingViewConfiguration)
        {
            var firstOpened = onboardingStorage.GetFirstOpened();
            if (firstOpened == null)
                return false;

            var targetDayCount = ratingViewConfiguration.DayCount;
            var actualDayCount = (timeService.CurrentDateTime - firstOpened.Value).TotalDays;
            return actualDayCount >= targetDayCount;
        }

        private IObservable<(bool criterionMatched, RatingViewConfiguration configuration)> criterionMatched(RatingViewConfiguration configuration)
        {
            switch (configuration.Criterion)
            {
                case RatingViewCriterion.Stop:
                    return dataSource
                        .TimeEntries
                        .TimeEntryStopped
                        .Select(_ => (true, configuration))
                        .Take(1);

                case RatingViewCriterion.Start:
                    return dataSource
                        .TimeEntries
                        .TimeEntryStarted
                        .Select(_ => (true, configuration))
                        .Take(1);
                    
                case RatingViewCriterion.Continue:
                    return dataSource
                        .TimeEntries
                        .TimeEntryContinued
                        .Merge(dataSource.TimeEntries.SuggestionStarted)
                        .Select(_ => (true, configuration))
                        .Take(1);

                default:
                    return Observable.Return((false, configuration));
            }
        }
    }
}
