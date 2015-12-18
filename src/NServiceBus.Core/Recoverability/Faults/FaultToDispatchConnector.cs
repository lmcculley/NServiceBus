namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.Faults;
    using Routing;
    using Pipeline;
    using TransportDispatch;

    class FaultToDispatchConnector : StageConnector<IFaultContext, IRoutingContext>
    {
        public override Task Invoke(IFaultContext context, Func<IRoutingContext, Task> next)
        {
            var message = context.Message;

            State state;

            if (context.Extensions.TryGet(out state))
            {
                //transfer fault values to the headers of the message to fault
                foreach (var kvp in state.FayultValues)
                {
                    message.Headers[kvp.Key] = kvp.Value;
                }
            }

            var dispatchContext = new RoutingContext(message, new UnicastRoutingStrategy(context.ErrorQueueAddress), context);
            
            return next(dispatchContext);
        }

        public class State
        {
            public Dictionary<string, string> FayultValues = new Dictionary<string, string>();
        }
    }
}