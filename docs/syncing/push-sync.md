Push sync loop
==============

Pushing entities to the server is more complicated than pulling data and it requires a lot more code and more syncing states.

We push entities in the same order we pull them - for the same reason - we need to have the dependencies of some entity on the server before we start pushing this entity to avoid errors.

The loop
--------

For every entity type we define a simple loop: take the oldest unsynced entity, push it to the server, repeat until there are no more unsynced entities, then proceed to the loop for the next entity type. This logic is implemented in the `PushState` class.

Pushing a single entity then means choosing one of the 4 operations:
- the entity was *created* on the device and has not been synced yet -> create entity _on the server_
- the entity was *deleted* on the device and *has not been synced yet* -> delete it _on the device_
- the entity was *deleted* on the device and it exists on the server -> delete it _on the server_
- the entity was *updated* on the device -> update it on server

_Note: we only support all of these operations for time entries, the other entities can be either only created in the app but not updated or deleted (e.g., clients and projects) or only updated (preferences)._

Each of the individual states simply create a HTTP request and if it is successful then the local entity is updated with the entity data in the body of the HTTP response (using the `ignoreIfChangedLocally` conflict resolution).

If the server reports an error, we try to resolve the situation according to the type of error:
- for server errors we enter the retry loop
- for the client error `429 Too Many Requests` we also enter the retry loop
- for other client errors we mark the entity as unsyncable and skip it until the user resolves the error in the app

The retry loop works the same way it does in the pull loop - it uses the same code.

---

Next topic: [State machine](state-machine.md)