﻿namespace NServiceBus.AcceptanceTests.Msmq
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.Config;
    using NUnit.Framework;

    public class When_TimeToBeReceivedOnForwardedMessages_set_and_transactional_Msmq : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Endpoint_should_not_start_and_show_error()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<Endpoint>()
                .Done(c => c.EndpointsStarted)
                .Run();

            Assert.True(context.Exceptions.First().Message.Contains("Setting a custom TimeToBeReceivedOnForwardedMessages is not supported on transactional MSMQ."));
        }
        
        public class Context : ScenarioContext { }

        public class Endpoint : EndpointConfigurationBuilder
        {
            public Endpoint()
            {
                EndpointSetup<DefaultServer>((config, context) =>
                {
                    config.UseTransport(context.GetTransportType())
                            .Transactions(TransportTransactionMode.TransactionScope);
                })
                    .WithConfig<UnicastBusConfig>(c => c.TimeToBeReceivedOnForwardedMessages = TimeSpan.FromHours(1));
            }
        }
    }
}