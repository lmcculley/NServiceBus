namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.Logging;

    class PipelineCollection
    {
        public PipelineCollection(IEnumerable<TransportReceiver> pipelines)
        {
            this.pipelines = pipelines.ToArray();
        }

        public async Task Start()
        {
            foreach (var pipeline in pipelines)
            {
                Logger.DebugFormat("Starting {0} pipeline", pipeline.Id);
                try
                {
                    await pipeline.Start().ConfigureAwait(false);
                    Logger.DebugFormat("Started {0} pipeline", pipeline.Id);
                }
                catch (Exception ex)
                {
                    Logger.Fatal($"Pipeline {pipeline.Id} failed to start.", ex);
                    throw;
                }
            }
        }

        public Task Stop()
        {
            var pipelineStopTasks = pipelines.Select(async pipeline =>
            {
                Logger.DebugFormat("Stopping {0} pipeline", pipeline.Id);
                await pipeline.Stop().ConfigureAwait(false);
                Logger.DebugFormat("Stopped {0} pipeline", pipeline.Id);
            });

            return Task.WhenAll(pipelineStopTasks);
        }

        TransportReceiver[] pipelines;


        static ILog Logger = LogManager.GetLogger<PipelineCollection>();
    }
}