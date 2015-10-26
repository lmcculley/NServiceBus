namespace NServiceBus.Transports
{
    using NServiceBus.Features;
    using NServiceBus.Settings;

    class Sending : Feature
    {
        public Sending()
        {
            EnableByDefault();
            DependsOn<UnicastBus>();
            RegisterStartupTask<PerformTransportStartUpTests>();
        }
        
        protected internal override void Setup(FeatureConfigurationContext context)
        {
            var transport = context.Settings.Get<OutboundTransport>();
            context.Container.ConfigureComponent(c =>
            {
                var sendConfigContext = transport.Configure(context.Settings);
                var dispatcher = sendConfigContext.DispatcherFactory();
                return dispatcher;
            }, DependencyLifecycle.SingleInstance);
        }

        class PerformTransportStartUpTests : FeatureStartupTask
        {
            ReadOnlySettings settings;

            public PerformTransportStartUpTests(ReadOnlySettings settings)
            {
                this.settings = settings;
            }

            protected override void OnStart()
            {
                var queueBindings = settings.Get<QueueBindings>();
                var transportDef = settings.Get<TransportDefinition>();

                transportDef.PerformStartUpChecks(new TransportStartUpCheckContext(settings, queueBindings));
            }
        }

    }
}