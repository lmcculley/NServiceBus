namespace NServiceBus.Pipeline
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Unicast.Transport;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <typeparam name="TTo"></typeparam>
    /// <typeparam name="TFork"></typeparam>
    public abstract class StageForkConnector<TFrom, TTo, TFork> : IBehavior<TFrom, TTo>, IForkConnector<TFork>, IStageConnector
        where TFrom : IBehaviorContext
        where TTo : IBehaviorContext
        where TFork : IBehaviorContext
    {
        /// <summary>
        /// Contains information about the pipeline this behavior is part of.
        /// </summary>
        protected PipelineInfo PipelineInfo { get; private set; }

        /// <inheritdoc />
        public abstract Task Invoke(TFrom context, Func<TTo, Task> next, Func<TFork, Task> fork);

        /// <inheritdoc />
        public Task Invoke(TFrom context, Func<TTo, Task> next)
        {
            Guard.AgainstNull(nameof(context), context);
            Guard.AgainstNull(nameof(next), next);

            return Invoke(context, next, ctx =>
            {
                var cache = context.Extensions.Get<PipelineCache>();
                var pipeline = cache.Pipeline<TFork>();
                return pipeline.Invoke(ctx);
            });
        }

        /// <summary>
        /// Initialized the behavior with information about the just constructed pipeline.
        /// </summary>
        public void Initialize(PipelineInfo pipelineInfo)
        {
            PipelineInfo = pipelineInfo;
        }

        /// <summary>
        /// Allows a behavior to perform any necessary warm-up activities (such as priming a cache), possibly in an async way.
        /// </summary>
        public virtual Task Warmup()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Allows a behavior to perform any necessary cool-down activities, possibly in an async way.
        /// </summary>
        public virtual Task Cooldown()
        {
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <typeparam name="TFork"></typeparam>
    public abstract class ForkConnector<TFrom, TFork> : Behavior<TFrom>, IForkConnector<TFork>
        where TFrom : IBehaviorContext
        where TFork : IBehaviorContext
    {
        /// <inheritdoc />
        public abstract Task Invoke(TFrom context, Func<Task> next, Func<TFork, Task> fork);

        /// <inheritdoc />
        public sealed override Task Invoke(TFrom context, Func<Task> next)
        {
            Guard.AgainstNull(nameof(context), context);
            Guard.AgainstNull(nameof(next), next);

            return Invoke(context, next, ctx =>
            {
                var cache = context.Extensions.Get<PipelineCache>();
                var pipeline = cache.Pipeline<TFork>();
                return pipeline.Invoke(ctx);
            });
        }
    }
}