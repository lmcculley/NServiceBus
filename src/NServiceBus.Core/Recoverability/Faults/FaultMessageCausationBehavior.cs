namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Faults;
    using NServiceBus.Pipeline;

    class FaultMessageCausationBehavior : Behavior<IFaultContext>
    {
        public override Task Invoke(IFaultContext context, Func<Task> next)
        {
            string conversationId;
            if (context.Message.Headers.TryGetValue(Headers.ConversationId, out conversationId))
            {
                context.AddFaultData(Headers.ConversationId, conversationId);
            }

            string relatedTo;
            if (context.Message.Headers.TryGetValue(Headers.RelatedTo, out relatedTo))
            {
                context.AddFaultData(Headers.RelatedTo, context.Message.Headers[Headers.RelatedTo]);
            }

            return next();
        }
    }
}