using System;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;
using System.Reactive.Concurrency;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant.Models;
using Toggl.Foundation.DataSources;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.States.Push;
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
            ConfigureTransitions(transitions, database, api, dataSource, apiDelay, scheduler, timeService, entryPoints, delayCancellationObservable);
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
            StateMachineEntryPoints entryPoints,
            IObservable<Unit> delayCancellation)
        {
            configurePullTransitions(transitions, database, api, dataSource, timeService, scheduler, entryPoints.StartPullSync, delayCancellation);
            configurePushTransitions(transitions, api, dataSource, apiDelay, scheduler, entryPoints.StartPushSync, delayCancellation);
        }

        private static void configurePullTransitions(
            TransitionHandlerProvider transitions,
            ITogglDatabase database,
            ITogglApi api,
            ITogglDataSource dataSource,
            ITimeService timeService,
            IScheduler scheduler,
            StateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var fetchAllSince = new FetchAllSinceState(database, api, timeService);
            var persistWorkspaces = new PersistState<IWorkspace, IDatabaseWorkspace, IThreadSafeWorkspace>(dataSource.Workspaces, database.SinceParameters, Workspace.Clean);
            var persistWorkspaceFeatures = new PersistState<IWorkspaceFeatureCollection, IDatabaseWorkspaceFeatureCollection, IThreadSafeWorkspaceFeatureCollection>(
                dataSource.WorkspaceFeatures, database.SinceParameters, WorkspaceFeatureCollection.From);
            var persistUser = new PersistState<IUser, IDatabaseUser, IThreadSafeUser>(dataSource.User, database.SinceParameters, User.Clean);
            var persistTags = new PersistState<ITag, IDatabaseTag, IThreadSafeTag>(dataSource.Tags, database.SinceParameters, Tag.Clean);
            var persistClients = new PersistState<IClient, IDatabaseClient, IThreadSafeClient>(dataSource.Clients, database.SinceParameters, Client.Clean);
            var persistPreferences = new PersistState<IPreferences, IDatabasePreferences, IThreadSafePreferences>(dataSource.Preferences, database.SinceParameters, Preferences.Clean);
            var persistProjects = new PersistState<IProject, IDatabaseProject, IThreadSafeProject>(dataSource.Projects, database.SinceParameters, Project.Clean);
            var persistTimeEntries = new PersistState<ITimeEntry, IDatabaseTimeEntry, IThreadSafeTimeEntry>(dataSource.TimeEntries, database.SinceParameters, TimeEntry.Clean);
            var persistTasks = new PersistState<ITask, IDatabaseTask, IThreadSafeTask>(dataSource.Tasks, database.SinceParameters, Task.Clean);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            transitions.ConfigureTransition(entryPoint, fetchAllSince.Start);
            transitions.ConfigureTransition(fetchAllSince.FetchStarted, persistWorkspaces.Start);
            transitions.ConfigureTransition(persistWorkspaces.FinishedPersisting, persistUser.Start);
            transitions.ConfigureTransition(persistUser.FinishedPersisting, persistWorkspaceFeatures.Start);
            transitions.ConfigureTransition(persistWorkspaceFeatures.FinishedPersisting, persistPreferences.Start);
            transitions.ConfigureTransition(persistPreferences.FinishedPersisting, persistTags.Start);
            transitions.ConfigureTransition(persistTags.FinishedPersisting, persistClients.Start);
            transitions.ConfigureTransition(persistClients.FinishedPersisting, persistProjects.Start);
            transitions.ConfigureTransition(persistProjects.FinishedPersisting, persistTasks.Start);
            transitions.ConfigureTransition(persistTasks.FinishedPersisting, persistTimeEntries.Start);

            transitions.ConfigureTransition(persistWorkspaces.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistWorkspaceFeatures.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistPreferences.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistTags.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistClients.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistProjects.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistTasks.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistTimeEntries.Failed, checkServerStatus.Start);

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
            var create = new CreateEntityState<TModel, TDatabase, TThreadsafe>(creatingApi, dataSource, toClean);
            var update = new UpdateEntityState<TModel, TDatabase, TThreadsafe>(updatingApi, dataSource, toClean);
            var delete = new DeleteEntityState<TModel, TDatabase, TThreadsafe>(deletingApi, dataSource);
            var deleteLocal = new DeleteLocalEntityState<TDatabase, TThreadsafe>(dataSource);
            var tryResolveClientError = new TryResolveClientErrorState<TThreadsafe>();
            var unsyncable = new UnsyncableEntityState<TDatabase, TThreadsafe>(dataSource, toUnsyncable);
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
            var create = new CreateEntityState<TModel, TDatabase, TThreadsafe>(creatingApi, dataSource, toClean);
            var tryResolveClientError = new TryResolveClientErrorState<TThreadsafe>();
            var unsyncable = new UnsyncableEntityState<TDatabase, TThreadsafe>(dataSource, toUnsyncable);
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

        private static IStateResult configurePushSingleton<TModel, TDatabase, TThreadsafe>(
            TransitionHandlerProvider transitions,
            IStateResult entryPoint,
            ISingletonDataSource<TThreadsafe, TDatabase> dataSource,
            IUpdatingApiClient<TModel> updatingApi,
            Func<TModel, TThreadsafe> toClean,
            Func<TThreadsafe, string, TThreadsafe> toUnsyncable,
            ITogglApi api,
            IScheduler scheduler,
            IObservable<Unit> delayCancellation)
            where TModel : class
            where TDatabase : class, TModel, IDatabaseSyncable
            where TThreadsafe : class, TDatabase, IThreadSafeModel
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushSingleState<TDatabase, TThreadsafe>(dataSource);
            var pushOne = new PushOneEntityState<TThreadsafe>();
            var update = new UpdateEntityState<TModel, TDatabase, TThreadsafe>(updatingApi, dataSource, toClean);
            var tryResolveClientError = new TryResolveClientErrorState<TThreadsafe>();
            var unsyncable = new UnsyncableEntityState<TDatabase, TThreadsafe>(dataSource, toUnsyncable);
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
