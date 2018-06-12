using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Sync.States.Pull;
using Toggl.Foundation.Sync.States.Push;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave;
using Toggl.Ultrawave.ApiClients;
using Toggl.Ultrawave.ApiClients.Interfaces;

namespace Toggl.Foundation
{
    public static class TogglSyncManager
    {
        public static ISyncManager CreateSyncManager(
            ITogglDatabase database,
            ITogglApi api,
            ITogglDataSource dataSource,
            ITimeService timeService,
            IAnalyticsService analyticsService,
            TimeSpan? retryLimit,
            IScheduler scheduler)
        {
            var random = new Random();
            var queue = new SyncStateQueue();
            var entryPoints = new StateMachineEntryPoints();
            var transitions = new TransitionHandlerProvider();
            var apiDelay = new RetryDelayService(random, retryLimit);
            var delayCancellation = new Subject<Unit>();
            var delayCancellationObservable = delayCancellation.AsObservable().Replay();
            ConfigureTransitions(transitions, database, api, dataSource, apiDelay, scheduler, timeService, analyticsService, entryPoints, delayCancellationObservable);
            var stateMachine = new StateMachine(transitions, scheduler, delayCancellation);
            var orchestrator = new StateMachineOrchestrator(stateMachine, entryPoints);

            return new SyncManager(queue, orchestrator, analyticsService);
        }

        public static void ConfigureTransitions(
            TransitionHandlerProvider transitions,
            ITogglDatabase database,
            ITogglApi api,
            ITogglDataSource dataSource,
            IRetryDelayService apiDelay,
            IScheduler scheduler,
            ITimeService timeService,
            IAnalyticsService analyticsService,
            StateMachineEntryPoints entryPoints,
            IObservable<Unit> delayCancellation)
        {
            configurePullTransitions(transitions, database, api, dataSource, timeService, analyticsService, scheduler, entryPoints.StartPullSync, delayCancellation);
            configurePushTransitions(transitions, api, dataSource, apiDelay, scheduler, entryPoints.StartPushSync, delayCancellation);
        }

        private static void configurePullTransitions(
            TransitionHandlerProvider transitions,
            ITogglDatabase database,
            ITogglApi api,
            ITogglDataSource dataSource,
            ITimeService timeService,
            IAnalyticsService analyticsService,
            IScheduler scheduler,
            StateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var fetchAllSince = new FetchAllSinceState(database, api, timeService);

            var persistWorkspaces =
                new PersistListState<IWorkspace, IDatabaseWorkspace, IThreadSafeWorkspace>(dataSource.Workspaces, Workspace.Clean)
                    .UpdateSince<IWorkspace, IDatabaseWorkspace>(database.SinceParameters)
                    .CatchApiExceptions();

            var persistWorkspaceFeatures =
                new PersistListState<IWorkspaceFeatureCollection, IDatabaseWorkspaceFeatureCollection, IThreadSafeWorkspaceFeatureCollection>(
                        dataSource.WorkspaceFeatures, WorkspaceFeatureCollection.From)
                    .CatchApiExceptions();

            var persistUser =
                new PersistSingletonState<IUser, IDatabaseUser, IThreadSafeUser>(dataSource.User, User.Clean)
                    .CatchApiExceptions();

            var persistTags =
                new PersistListState<ITag, IDatabaseTag, IThreadSafeTag>(dataSource.Tags, Tag.Clean)
                    .UpdateSince<ITag, IDatabaseTag>(database.SinceParameters)
                    .CatchApiExceptions();

            var persistClients =
                new PersistListState<IClient, IDatabaseClient, IThreadSafeClient>(dataSource.Clients, Client.Clean)
                    .UpdateSince<IClient, IDatabaseClient>(database.SinceParameters)
                    .CatchApiExceptions();

            var persistPreferences =
                new PersistSingletonState<IPreferences, IDatabasePreferences, IThreadSafePreferences>(dataSource.Preferences, Preferences.Clean)
                    .CatchApiExceptions();

            var persistProjects =
                new PersistListState<IProject, IDatabaseProject, IThreadSafeProject>(dataSource.Projects, Project.Clean)
                    .UpdateSince<IProject, IDatabaseProject>(database.SinceParameters)
                    .CatchApiExceptions();

            var createGhostProjects = new CreateGhostProjectsState(dataSource.Projects, analyticsService).CatchApiExceptions();

            var persistTimeEntries =
                new PersistListState<ITimeEntry, IDatabaseTimeEntry, IThreadSafeTimeEntry>(dataSource.TimeEntries, TimeEntry.Clean)
                    .UpdateSince<ITimeEntry, IDatabaseTimeEntry>(database.SinceParameters)
                    .CatchApiExceptions();

            var persistTasks =
                new PersistListState<ITask, IDatabaseTask, IThreadSafeTask>(dataSource.Tasks, Task.Clean)
                    .UpdateSince<ITask, IDatabaseTask>(database.SinceParameters)
                    .CatchApiExceptions();

            var refetchInaccessibleProjects =
                new TryFetchInaccessibleProjectsState(dataSource.Projects, timeService, api.Projects)
                    .CatchApiExceptions();

            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);

            var finished = new ResetAPIDelayState(apiDelay);
            var deleteOlderEntries = new DeleteOldEntriesState(timeService, dataSource.TimeEntries);
            var deleteNonReferencedGhostProjects = new DeleteNonReferencedProjectGhostsState(dataSource.Projects, dataSource.TimeEntries);

            transitions.ConfigureTransition(entryPoint, fetchAllSince.Start);
            transitions.ConfigureTransition(fetchAllSince.FetchStarted, persistWorkspaces.Start);
            transitions.ConfigureTransition(persistWorkspaces.UnsafeState.FinishedPersisting, persistUser.Start);
            transitions.ConfigureTransition(persistUser.UnsafeState.FinishedPersisting, persistWorkspaceFeatures.Start);
            transitions.ConfigureTransition(persistWorkspaceFeatures.UnsafeState.FinishedPersisting, persistPreferences.Start);
            transitions.ConfigureTransition(persistPreferences.UnsafeState.FinishedPersisting, persistTags.Start);
            transitions.ConfigureTransition(persistTags.UnsafeState.FinishedPersisting, persistClients.Start);
            transitions.ConfigureTransition(persistClients.UnsafeState.FinishedPersisting, persistProjects.Start);
            transitions.ConfigureTransition(persistProjects.UnsafeState.FinishedPersisting, persistTasks.Start);
            transitions.ConfigureTransition(persistTasks.UnsafeState.FinishedPersisting, createGhostProjects.Start);
            transitions.ConfigureTransition(createGhostProjects.UnsafeState.FinishedPersisting, persistTimeEntries.Start);
            transitions.ConfigureTransition(persistTimeEntries.UnsafeState.FinishedPersisting, refetchInaccessibleProjects.Start);
            transitions.ConfigureTransition(refetchInaccessibleProjects.UnsafeState.FetchNext, refetchInaccessibleProjects.Start);

            transitions.ConfigureTransition(refetchInaccessibleProjects.UnsafeState.FinishedPersisting, deleteOlderEntries.Start);
            transitions.ConfigureTransition(deleteOlderEntries.FinishedDeleting, deleteNonReferencedGhostProjects.Start);

            transitions.ConfigureTransition(persistWorkspaces.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistUser.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistWorkspaceFeatures.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistPreferences.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistTags.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistClients.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistProjects.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistTasks.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(createGhostProjects.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistTimeEntries.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(refetchInaccessibleProjects.Failed, checkServerStatus.Start);

            transitions.ConfigureTransition(checkServerStatus.Retry, checkServerStatus.Start);
            transitions.ConfigureTransition(checkServerStatus.ServerIsAvailable, finished.Start);
            transitions.ConfigureTransition(finished.Continue, fetchAllSince.Start);
        }

        private static void configurePushTransitions(
            TransitionHandlerProvider transitions,
            ITogglApi api,
            ITogglDataSource dataSource,
            IRetryDelayService apiDelay,
            IScheduler scheduler,
            StateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var pushingUsersFinished = configurePushSingleton(transitions, entryPoint, dataSource.User, api.User, User.Clean, User.Unsyncable, api, scheduler, delayCancellation);
            var pushingPreferencesFinished = configurePushSingleton(transitions, pushingUsersFinished, dataSource.Preferences, api.Preferences, Preferences.Clean, Preferences.Unsyncable, api, scheduler, delayCancellation);
            var pushingTagsFinished = configureCreateOnlyPush(transitions, pushingPreferencesFinished, dataSource.Tags, api.Tags, Tag.Clean, Tag.Unsyncable, api, scheduler, delayCancellation);
            var pushingClientsFinished = configureCreateOnlyPush(transitions, pushingTagsFinished, dataSource.Clients, api.Clients, Client.Clean, Client.Unsyncable, api, scheduler, delayCancellation);
            var pushingProjectsFinished = configureCreateOnlyPush(transitions, pushingClientsFinished, dataSource.Projects, api.Projects, Project.Clean, Project.Unsyncable, api, scheduler, delayCancellation);
            configurePush(transitions, pushingProjectsFinished, dataSource.TimeEntries, api.TimeEntries, api.TimeEntries, api.TimeEntries, TimeEntry.Clean, TimeEntry.Unsyncable, api, apiDelay, scheduler, delayCancellation);
        }

        private static IStateResult configurePush<TModel, TDatabase, TThreadsafe>(
            TransitionHandlerProvider transitions,
            IStateResult entryPoint,
            IDataSource<TThreadsafe, TDatabase> dataSource,
            ICreatingApiClient<TModel> creatingApi,
            IUpdatingApiClient<TModel> updatingApi,
            IDeletingApiClient<TModel> deletingApi,
            Func<TModel, TThreadsafe> toClean,
            Func<TThreadsafe, string, TThreadsafe> toUnsyncable,
            ITogglApi api,
            IRetryDelayService apiDelay,
            IScheduler scheduler,
            IObservable<Unit> delayCancellation)
            where TModel : class, IIdentifiable, ILastChangedDatable
            where TDatabase : class, TModel, IDatabaseSyncable
            where TThreadsafe : class, TDatabase, IThreadSafeModel
        {
            var rnd = new Random();
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushState<TDatabase, TThreadsafe>(dataSource);
            var pushOne = new PushOneEntityState<TThreadsafe>();
            var create = new CreateEntityState<TModel, TThreadsafe>(creatingApi, dataSource, toClean);
            var update = new UpdateEntityState<TModel, TThreadsafe>(updatingApi, dataSource, toClean);
            var delete = new DeleteEntityState<TModel, TDatabase, TThreadsafe>(deletingApi, dataSource);
            var deleteLocal = new DeleteLocalEntityState<TDatabase, TThreadsafe>(dataSource);
            var tryResolveClientError = new TryResolveClientErrorState<TThreadsafe>();
            var unsyncable = new UnsyncableEntityState<TThreadsafe>(dataSource, toUnsyncable);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            transitions.ConfigureTransition(entryPoint, push.Start);
            transitions.ConfigureTransition(push.PushEntity, pushOne.Start);
            transitions.ConfigureTransition(pushOne.CreateEntity, create.Start);
            transitions.ConfigureTransition(pushOne.UpdateEntity, update.Start);
            transitions.ConfigureTransition(pushOne.DeleteEntity, delete.Start);
            transitions.ConfigureTransition(pushOne.DeleteEntityLocally, deleteLocal.Start);

            transitions.ConfigureTransition(create.ClientError, tryResolveClientError.Start);
            transitions.ConfigureTransition(update.ClientError, tryResolveClientError.Start);
            transitions.ConfigureTransition(delete.ClientError, tryResolveClientError.Start);

            transitions.ConfigureTransition(create.ServerError, checkServerStatus.Start);
            transitions.ConfigureTransition(update.ServerError, checkServerStatus.Start);
            transitions.ConfigureTransition(delete.ServerError, checkServerStatus.Start);

            transitions.ConfigureTransition(create.UnknownError, checkServerStatus.Start);
            transitions.ConfigureTransition(update.UnknownError, checkServerStatus.Start);
            transitions.ConfigureTransition(delete.UnknownError, checkServerStatus.Start);

            transitions.ConfigureTransition(tryResolveClientError.UnresolvedTooManyRequests, checkServerStatus.Start);
            transitions.ConfigureTransition(tryResolveClientError.Unresolved, unsyncable.Start);

            transitions.ConfigureTransition(checkServerStatus.Retry, checkServerStatus.Start);
            transitions.ConfigureTransition(checkServerStatus.ServerIsAvailable, push.Start);

            transitions.ConfigureTransition(create.CreatingFinished, finished.Start);
            transitions.ConfigureTransition(update.UpdatingSucceeded, finished.Start);
            transitions.ConfigureTransition(delete.DeletingFinished, finished.Start);
            transitions.ConfigureTransition(deleteLocal.Deleted, finished.Start);
            transitions.ConfigureTransition(deleteLocal.DeletingFailed, finished.Start);

            transitions.ConfigureTransition(finished.Continue, push.Start);

            return push.NothingToPush;
        }

        private static IStateResult configureCreateOnlyPush<TModel, TDatabase, TThreadsafe>(
            TransitionHandlerProvider transitions,
            IStateResult entryPoint,
            IDataSource<TThreadsafe, TDatabase> dataSource,
            ICreatingApiClient<TModel> creatingApi,
            Func<TModel, TThreadsafe> toClean,
            Func<TThreadsafe, string, TThreadsafe> toUnsyncable,
            ITogglApi api,
            IScheduler scheduler,
            IObservable<Unit> delayCancellation)
            where TModel : IIdentifiable, ILastChangedDatable
            where TDatabase : class, TModel, IDatabaseSyncable
            where TThreadsafe : class, TDatabase, IThreadSafeModel
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushState<TDatabase, TThreadsafe>(dataSource);
            var pushOne = new PushOneEntityState<TThreadsafe>();
            var create = new CreateEntityState<TModel, TThreadsafe>(creatingApi, dataSource, toClean);
            var tryResolveClientError = new TryResolveClientErrorState<TThreadsafe>();
            var unsyncable = new UnsyncableEntityState<TThreadsafe>(dataSource, toUnsyncable);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            transitions.ConfigureTransition(entryPoint, push.Start);
            transitions.ConfigureTransition(push.PushEntity, pushOne.Start);
            transitions.ConfigureTransition(pushOne.CreateEntity, create.Start);

            transitions.ConfigureTransition(pushOne.UpdateEntity, new InvalidTransitionState($"Updating is not supported for {typeof(TModel).Name} during Push sync.").Start);
            transitions.ConfigureTransition(pushOne.DeleteEntity, new InvalidTransitionState($"Deleting is not supported for {typeof(TModel).Name} during Push sync.").Start);
            transitions.ConfigureTransition(pushOne.DeleteEntityLocally, new InvalidTransitionState($"Deleting locally is not supported for {typeof(TModel).Name} during Push sync.").Start);

            transitions.ConfigureTransition(create.ClientError, tryResolveClientError.Start);
            transitions.ConfigureTransition(create.ServerError, checkServerStatus.Start);
            transitions.ConfigureTransition(create.UnknownError, checkServerStatus.Start);

            transitions.ConfigureTransition(tryResolveClientError.UnresolvedTooManyRequests, checkServerStatus.Start);
            transitions.ConfigureTransition(tryResolveClientError.Unresolved, unsyncable.Start);

            transitions.ConfigureTransition(checkServerStatus.Retry, checkServerStatus.Start);
            transitions.ConfigureTransition(checkServerStatus.ServerIsAvailable, push.Start);

            transitions.ConfigureTransition(create.CreatingFinished, finished.Start);

            transitions.ConfigureTransition(finished.Continue, push.Start);

            return push.NothingToPush;
        }

        private static IStateResult configurePushSingleton<TModel, TThreadsafe>(
            TransitionHandlerProvider transitions,
            IStateResult entryPoint,
            ISingletonDataSource<TThreadsafe> dataSource,
            IUpdatingApiClient<TModel> updatingApi,
            Func<TModel, TThreadsafe> toClean,
            Func<TThreadsafe, string, TThreadsafe> toUnsyncable,
            ITogglApi api,
            IScheduler scheduler,
            IObservable<Unit> delayCancellation)
            where TModel : class
            where TThreadsafe : class, TModel, IThreadSafeModel, IDatabaseSyncable
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushSingleState<TThreadsafe>(dataSource);
            var pushOne = new PushOneEntityState<TThreadsafe>();
            var update = new UpdateEntityState<TModel, TThreadsafe>(updatingApi, dataSource, toClean);
            var tryResolveClientError = new TryResolveClientErrorState<TThreadsafe>();
            var unsyncable = new UnsyncableEntityState<TThreadsafe>(dataSource, toUnsyncable);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            transitions.ConfigureTransition(entryPoint, push.Start);
            transitions.ConfigureTransition(push.PushEntity, pushOne.Start);
            transitions.ConfigureTransition(pushOne.UpdateEntity, update.Start);

            transitions.ConfigureTransition(pushOne.CreateEntity, new InvalidTransitionState($"Creating is not supported for {typeof(TModel).Name} during Push sync.").Start);
            transitions.ConfigureTransition(pushOne.DeleteEntity, new InvalidTransitionState($"Deleting is not supported for {typeof(TModel).Name} during Push sync.").Start);
            transitions.ConfigureTransition(pushOne.DeleteEntityLocally, new InvalidTransitionState($"Deleting locally is not supported for {typeof(TModel).Name} during Push sync.").Start);

            transitions.ConfigureTransition(update.ClientError, tryResolveClientError.Start);
            transitions.ConfigureTransition(update.ServerError, checkServerStatus.Start);
            transitions.ConfigureTransition(update.UnknownError, checkServerStatus.Start);

            transitions.ConfigureTransition(tryResolveClientError.UnresolvedTooManyRequests, checkServerStatus.Start);
            transitions.ConfigureTransition(tryResolveClientError.Unresolved, unsyncable.Start);

            transitions.ConfigureTransition(checkServerStatus.Retry, checkServerStatus.Start);
            transitions.ConfigureTransition(checkServerStatus.ServerIsAvailable, push.Start);

            transitions.ConfigureTransition(update.UpdatingSucceeded, finished.Start);

            transitions.ConfigureTransition(finished.Continue, push.Start);

            return push.NothingToPush;
        }
    }
}
