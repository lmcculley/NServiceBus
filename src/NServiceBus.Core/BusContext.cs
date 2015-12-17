namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Extensibility;
    using NServiceBus.Pipeline;

    class BusContext : IBusContext
    {
        public BusContext(IBehaviorContext context)
        {
            this.context = context;
        }

        public ContextBag Extensions => context.Extensions;

        public Task Send(object message, SendOptions options)
        {
            return BusOperations.Send(context, message, options);
        }

        public Task Send<T>(Action<T> messageConstructor, SendOptions options)
        {
            return BusOperations.Send(context, messageConstructor, options);
        }

        public Task Publish(object message, PublishOptions options)
        {
            return BusOperations.Publish(context, message, options);
        }

        public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
        {
            return BusOperations.Publish(context, messageConstructor, publishOptions);
        }

        public Task Subscribe(Type eventType, SubscribeOptions options)
        {
            return BusOperations.Subscribe(context, eventType, options);
        }

        public Task Unsubscribe(Type eventType, UnsubscribeOptions options)
        {
            return BusOperations.Unsubscribe(context, eventType, options);
        }

        IBehaviorContext context;
    }
}