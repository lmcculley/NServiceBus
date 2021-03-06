namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Features;
    using NServiceBus.Routing;
    using NServiceBus.Transports;

    class Receiving : Feature
    {
        internal Receiving()
        {
            EnableByDefault();
            DependsOn<Transport>();
            Prerequisite(c => !c.Settings.GetOrDefault<bool>("Endpoint.SendOnly"), "Endpoint is configured as send-only");
            Defaults(s =>
            {
                var transportAddresses = s.Get<TransportAddresses>();
                var userDiscriminator = s.GetOrDefault<string>("EndpointInstanceDiscriminator");

                if (userDiscriminator != null)
                {
                    var p = s.Get<TransportInfrastructure>().BindToLocalEndpoint(new EndpointInstance(s.EndpointName(), userDiscriminator));
                    s.SetDefault("NServiceBus.EndpointSpecificQueue", transportAddresses.GetTransportAddress(new LogicalAddress(p)));
                }
                var instanceProperties = s.Get<TransportInfrastructure>().BindToLocalEndpoint(new EndpointInstance(s.EndpointName()));
                s.SetDefault("NServiceBus.SharedQueue", transportAddresses.GetTransportAddress(new LogicalAddress(instanceProperties)));

                s.SetDefault<EndpointInstance>(instanceProperties);
            });
        }

        /// <summary>
        /// <see cref="Feature.Setup"/>.
        /// </summary>
        protected internal override void Setup(FeatureConfigurationContext context)
        {
            var inboundTransport = context.Settings.Get<InboundTransport>();

            context.Settings.Get<QueueBindings>().BindReceiving(context.Settings.LocalAddress());

            var instanceSpecificQueue = context.Settings.InstanceSpecificQueue();
            if (instanceSpecificQueue != null)
            {
                context.Settings.Get<QueueBindings>().BindReceiving(instanceSpecificQueue);
            }

            var lazyReceiveConfigResult = new Lazy<TransportReceiveInfrastructure>(()=> inboundTransport.Configure(context.Settings));
            context.Container.ConfigureComponent(b => lazyReceiveConfigResult.Value.MessagePumpFactory(), DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureComponent(b => lazyReceiveConfigResult.Value.QueueCreatorFactory(), DependencyLifecycle.SingleInstance);

            context.RegisterStartupTask(new PrepareForReceiving(lazyReceiveConfigResult));
        }
        
        class PrepareForReceiving : FeatureStartupTask
        {
            readonly Lazy<TransportReceiveInfrastructure> lazy;

            public PrepareForReceiving(Lazy<TransportReceiveInfrastructure> lazy)
            {
                this.lazy = lazy;
            }

            protected override async Task OnStart(IMessageSession session)
            {
                var result = await lazy.Value.PreStartupCheck().ConfigureAwait(false);
                if (!result.Succeeded)
                {
                    throw new Exception($"Pre start-up check failed: {result.ErrorMessage}");
                }
            }

            protected override Task OnStop(IMessageSession session)
            {
                return TaskEx.CompletedTask;
            }
        }
    }
}
