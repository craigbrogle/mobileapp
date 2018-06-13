using System.Collections.Generic;
using Toggl.Multivac;

namespace Toggl.Foundation.Sync
{
    public sealed class TransitionHandlerProvider : ITransitionHandlerProvider, ITransitionConfigurator
    {
        private readonly Dictionary<IStateResult, TransitionHandler> transitionHandlers
            = new Dictionary<IStateResult, TransitionHandler>();

        public void ConfigureTransition(IStateResult result, ISyncState state)
        {
            Ensure.Argument.IsNotNull(result, nameof(result));
            Ensure.Argument.IsNotNull(state, nameof(state));

            transitionHandlers.Add(result, _ => state.Start());
        }

        public void ConfigureTransition<T>(StateResult<T> result, ISyncState<T> state)
        {
            Ensure.Argument.IsNotNull(result, nameof(result));
            Ensure.Argument.IsNotNull(state, nameof(state));

            transitionHandlers.Add(
                result,
                t => state.Start(((Transition<T>)t).Parameter)
            );
        }

        public TransitionHandler GetTransitionHandler(IStateResult result)
        {
            Ensure.Argument.IsNotNull(result, nameof(result));

            transitionHandlers.TryGetValue(result, out var handler);
            return handler;
        }
    }
}
