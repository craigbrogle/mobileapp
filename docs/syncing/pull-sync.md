Pull sync loop
==============

Pull sync loop is responsible for querying the server and merging the obtained entities into the local database.

Limiting the queries with 'since' parameters
--------------------------------------------

To limit the amount of data downloaded from the server we use the URL variants which include the `since` parameter. Backend accepts since dates approximately 3 months into the past - we don't use dates older than two months, just to be sure.

If a `since` date is not available in the database or it is outdated, we fetch all the entities and update our `since` date in the database.

We calculate the `since` date by selecting the latest `ILastChangeDatable.At` value among all the pulled entities  (_note: this might yield a since date which is older than two months and we will fetch all of the entities even the next time - there is currently nothing we could do about it, using the device's current time is risky because it might be incorrect and we might skip fetching some data if the device was ahead of the server_).

The order of persisting entities
--------------------------------

To make sure that the all the dependencies are already persisted, we process them in a given order:

1. Workspaces
2. User _(depends on workspaces)_
3. Workspaces' features _(depends on workspaces)_
4. Preferences
5. Tags  _(depends on workspaces)_
6. Clients  _(depends on workspaces)_
7. Projects _(depends on workspaces and clients)_
8. Tasks _(depends on workspaces, projects and user)_
9. Time entries _(depends on workspaces, projects, tasks, tags)_

Conflict resolution
-------------------

We use conflict resolution and rivals resolution to avoid any data inconsistencies in our database. There is a dedicated chapter which describes the conflict resolution algorithms.

Ghost entities
--------------

Time entries keep references to the projects even after the projects were archived (and possibly in other scenarios), this means that we cannot rely on the projects being in the database even if we respect the order of persisting the entities by types.

To prevent the app from crashing and to bring up the UX of the app, we create "ghost entities" 👻 for projects which are referenced by time entries but are not in the database. We then later try to fetch the details of these projects using the reports API.

Pruning old data
----------------

After all data is pulled and persisted, we remove unnecessary data as part of the pull-sync loop.

We remove any time entry which was started more than two months ago.

We remove any ghost project which is not referenced by any time entry in the database.

_Note: we might move the pruning to a separate loop in the future._

Retry loop
----------

When a `ServerErrorException` or `ClientErrorException` other than `ApiDeprecatedException`, `ClientDeprecatedException` or `UnauthorizedException` is thrown during the processing of the HTTP request, the retry loop is entered.

The retry loop checks what the `/status` endpoint of the API server returns:
- 200 OK - exit the retry loop
- 500 Internal server error - wait for the next "slow delay" and try again
    - slow delay starts with 60 seconds and then is calculated: `previousDelay * rand(1.5, 2)`
- otherwise wait for the next "fast delay" and try again
    - fast delay starts with 10 seconds and then is calculated: `previousDelay * rand(1, 1.5)`


Where everything is implemented in the code
-------------------------------------------

The code is located (mostly) under the namespace `Toggl.Foundation.Sync.States.Pull`.

We initiate the HTTP requests in the state class `FetchAllSinceState`.

Individual states are instances of the `PersistSingletonState` (user, preferences) and `PersistListState` (the rest).

This basic logic is then wrapped in `SinceDateUpdatingPersistState` for the entities for which we store the `since` date. All states are wrapped with `ApiExceptionCatchingPersistState` which catches known exceptions and leads into the retry loop.

The logic of creating project ghosts is implemented in the `CreateGhostProjectsState` and fetching the details of these projects using the reports API is done in `TryFetchInaccessibleProjectsState`.

Retry loop uses the `CheckServerStatusState` and the `ResetAPIDelayState`.

The states are instantiated and connected in the `Toggl.Foundation.TogglSyncManagerFactory` class.