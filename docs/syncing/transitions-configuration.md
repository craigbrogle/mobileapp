Transitions configuration
=========================

Every sync state object has several "state results" - the possible outcomes of the operation. It is important that these states are instantiated just once and are never changed - we use reference equality to match state results.

Every syncing state is just a simple function which may take some parameter (result of the previous state) an returns an observable of a transition.

A transition is just a container which holds reference to the state result and it might contain also a parameter for the state result.

We use the state result to look for the next state in a dictionary-wrapper called `TransitionHandlerProvider`. We pick a transition handler (by convention it is the `Start` method of some state) and call it the parameter specified in the transition (if any).

We configure the coupling between state results with their handlers in `TogglSyncManagerFactory`.