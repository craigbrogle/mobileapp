Conflict resolution
===================

We need to merge our local state of an entity with the state we get from the server.

A conflict resolver is a function with this signature:

```csharp
ConflictResolutionMode Resolve<T>(T localEntity, T serverEntity);
```

There are 4 resolution modes:

- `Create`
- `Update`
- `Delete`
- `Ignore`

We use several conflict resolvers:

| Class name | Where it is used | What does it do |
|:---------- |:---------------- |:----------------|
| `AlwaysOverwrite` | Used for pulling workspace features. | If there is already an existing local entity - update it, otherwise create a new one. |
| `OverwriteUnlessNeedsSync` | Used for pulling preferences. | If there is no local entity, create it. When there is a local entity and it needs sync, ignore the server data, otherwise update the local entity. |
| `safeAlwaysDelete` | For the `DeleteAll` method of the `IDataSource` | Deletes everything. |
| `ignoreIfChangedLocally` | Used during push sync for overwriting local data with the data from the response from the server. | Checks the local entity agains some earlier state of that entity (in the push sync state we check against the state before the HTTP request was sent) and overrides the local data if the local entity has not change and ignores it otherwise. |
| `PreferNewer` | Used for pulling the rest of the entities. | <ul><li>Checks which entity was updated most recently (using `ILastChangedDatable.At`) and uses the data from this entity.</li><li>This resolver also checks if the entity on server was deleted or not (for deletable entities).</li><li>Local entities which are in sync or are marked as "refetching needed" - ghosts - are always overwritten.</li><li>An extra parameter `TimeSpan MarginOfError` can be used to acount for network delays ("5 second rule").</li></ul> |


Rivals resolution
-----------------

There is a second type of conflict resolution - so called "rivals" resolution. The difference is that the previously described conflict resolution is between the local and server version of the same entity, rivals are two entities of the same type which are in conflict.

Currently we use this only for a single purpose - to make sure there is just one running time entry - but we could use this concept to prevent projects/tags/clients with the same name in the same workspace.

A resolver must implement the interface `IRivalsResolver<T>`.

### Time entries rivals resolution

If two time entries are rivals (both of them have the `Duration` set to `null`) one of them will be stopped.

The time entry which was updated most recently will be left running and the other one will be stopped. We use the `ILastChangedDatable.At` to check which time entry is more up-to-date.

Selecting the stop time (duration) for the stopped time entry `A` is a complex process - from all of the time entries which start after the start of `A` we select the one with the minimum value or the current time in case of the list was empty.

_Implementation detail: To run the Realm query efficiently we create our own expression for evaluating which time entries start after a given time entry._