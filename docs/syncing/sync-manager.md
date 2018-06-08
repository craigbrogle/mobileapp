Sync Manager
============

The sync manager is the client-facing part of the syncing algorithm. It provides an interface to initiate synchronization (push, or full sync) and a way of observing the progress of syncing. It also handles errors which occur during evaluation of the sync operations.

The sync manager can run only one syncing operation at a time and if the client requests start of a sync operation while another one is already running, it will be queued and will be processed later.

Observing the progress
----------------------

The state of the syncing algorithm can be one of these:

- `Unknown` (only briefly at startup before the first sync operation is started)
- `Syncing`
- `Synced`
- `OfflineModeDetected`
- `Failed`

Use the `ProgressObservable` to monitor the state of the syncing algorithm.

Error handling
--------------

Several fatal errors will cause freezing of the manager and the underlying state machine:

- `ClientDeprecatedException` - the client app is not supported by the backend and the user must update the app
- `ApiDeprecatedException` - the API version is not supported anymore and the user must update the app to a newer version
- `UnauthorizedException` - the API token was revoked and the user is considered "logged out" and must enter his password again