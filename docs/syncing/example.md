Sample sync state
=================

Below you will find an example of a state which would make an API request and return a different result based on the response.

```csharp

public sealed class ServerStatus
{
    private readonly IApi api;

    private IStateResult ServerIsUp { get; } = new StateResult();

    private IStateResult<Exception> ServerIsDown { get; } = new StateResult();

    public ServerStatus(IApi api)
    {
        this.api = api;
    }

    public IObservable<ITransition> Start(Exception previousError = null)
    {
        var delay = previousError == null ? TimeSpan.FromSeconds(10) : TimeSpan.Zero;
        return api.CheckServerStatus() // returns IObservable<Unit>
            .Delay(delay)
            .Select(_ => ServerIsUp.Transition())
            .Catch((Exception exception) => Observable.Return(ServerIsDown.Transition(exception)));
    }
}

```

We could configure this state to loop if it fails:

```csharp
var entryPoint = new StateResult();
var otherState = new SomeState(api, database);
var serverStatus = new ServerStatus(api);
var transitions = new TransitionHandlerProvider();

transitions.ConfigureTransition(entryPoint, otherState.Start);
transitions.ConfigureTransition(otherState.FailureResult, serverStatus.Start);

transitions.ConfigureTransition(serverStatus.ServerIsUp, otherState.Start);
transitions.ConfigureTransition(serverStatus.ServerIsDown, serverStatus.Start);

// ...

stateMachine.Start(entryPoint.Transition());
```

---

This is the end of the syncing docs - you can go back to the [index](index.md).