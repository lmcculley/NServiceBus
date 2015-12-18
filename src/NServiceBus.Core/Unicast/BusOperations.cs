namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.MessageInterfaces;
    using NServiceBus.OutgoingPipeline;
    using NServiceBus.Pipeline;
    using NServiceBus.Routing;

    static class BusOperations
    {
        public static Task Publish<T>(IBehaviorContext context, Action<T> messageConstructor, PublishOptions options)
        {
            var mapper = context.Builder.Build<IMessageMapper>();
            return Publish(context, mapper.CreateInstance(messageConstructor), options);
        }

        public static Task Publish(IBehaviorContext context, object message, PublishOptions options)
        {
            var cache = context.Extensions.Get<PipelineCache>();
            var pipeline = cache.Pipeline<IOutgoingPublishContext>();

            var publishContext = new OutgoingPublishContext(
                new OutgoingLogicalMessage(message),
                options,
                context);

            return pipeline.Invoke(publishContext);
        }

        public static Task Subscribe(IBehaviorContext context, Type eventType, SubscribeOptions options)
        {
            var cache = context.Extensions.Get<PipelineCache>();
            var pipeline = cache.Pipeline<ISubscribeContext>();

            var subscribeContext = new SubscribeContext(
                context,
                eventType,
                options);

            return pipeline.Invoke(subscribeContext);
        }

        public static Task Unsubscribe(IBehaviorContext context, Type eventType, UnsubscribeOptions options)
        {
            var cache = context.Extensions.Get<PipelineCache>();
            var pipeline = cache.Pipeline<IUnsubscribeContext>();

            var subscribeContext = new UnsubscribeContext(
                context,
                eventType,
                options);

            return pipeline.Invoke(subscribeContext);
        }

        public static Task Send<T>(IBehaviorContext context, Action<T> messageConstructor, SendOptions options)
        {
            var mapper = context.Builder.Build<IMessageMapper>();
            return Send(context, mapper.CreateInstance(messageConstructor), options);
        }

        public static Task Send(IBehaviorContext context, object message, SendOptions options)
        {
            var messageType = message.GetType();

            return context.SendMessage(messageType, message, options);
        }

        static Task SendMessage(this IBehaviorContext context, Type messageType, object message, SendOptions options)
        {
            var cache = context.Extensions.Get<PipelineCache>();
            var pipeline = cache.Pipeline<IOutgoingSendContext>();

            var outgoingContext = new OutgoingSendContext(
                new OutgoingLogicalMessage(messageType, message),
                options,
                context);

            return pipeline.Invoke(outgoingContext);
        }

        public static Task Reply(IBehaviorContext context, object message, ReplyOptions options)
        {
            var cache = context.Extensions.Get<PipelineCache>();
            var pipeline = cache.Pipeline<IOutgoingReplyContext>();

            var outgoingContext = new OutgoingReplyContext(
                new OutgoingLogicalMessage(message),
                options,
                context);

            return pipeline.Invoke(outgoingContext);
        }

        public static Task Reply<T>(IBehaviorContext context, Action<T> messageConstructor, ReplyOptions options)
        {
            var mapper = context.Builder.Build<IMessageMapper>();
            return Reply(context, mapper.CreateInstance(messageConstructor), options);
        }
    }
}