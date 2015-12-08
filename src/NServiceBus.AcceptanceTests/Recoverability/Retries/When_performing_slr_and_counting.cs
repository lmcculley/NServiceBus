namespace NServiceBus.AcceptanceTests.Recoverability.Retries
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.Config;
    using NServiceBus.Features;
    using NUnit.Framework;

    public class When_performing_slr_and_counting : NServiceBusAcceptanceTest
    {

        [Test]
        public async Task Should_reschedule_message_three_times_by_default()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<RetryEndpoint>(b => b
                    .When(bus => bus.SendLocal(new MessageToBeRetried()))
                    .DoNotFailOnErrorMessages())
                .Done(c => RetryEndpoint.SentToErrorQueue)
                .Run();

            Assert.AreEqual(3, context.Logs.Count(l => l.Message
                .StartsWith($"Second Level Retry will reschedule message '{context.PhysicalMessageId}'")));
            Assert.AreEqual(1, context.Logs.Count(l => l.Message
                .StartsWith($"Giving up Second Level Retries for message '{context.PhysicalMessageId}'.")));
        }
        public class Context : ScenarioContext
        {
            public string PhysicalMessageId { get; set; }
        }

        public class RetryEndpoint : EndpointConfigurationBuilder
        {
            public RetryEndpoint()
            {
                EndpointSetup<DefaultServer>(configure =>
                {
                    configure.DisableFeature<FirstLevelRetries>();
                    configure.EnableFeature<SecondLevelRetries>();
                    configure.EnableFeature<TimeoutManager>();
                    configure.NotifyOnFailedMessage(message => RetryEndpoint.SentToErrorQueue = true);
                })
                .WithConfig<SecondLevelRetriesConfig>(c => c.TimeIncrease = TimeSpan.FromSeconds(1));
            }
            public static bool SentToErrorQueue;


            class MessageToBeRetriedHandler : IHandleMessages<MessageToBeRetried>
            {
                public Context TestContext { get; set; }

                public Task Handle(MessageToBeRetried message, IMessageHandlerContext context)
                {
                    TestContext.PhysicalMessageId = context.MessageId;
                    throw new SimulatedException();
                }
            }
        }

        [Serializable]
        public class MessageToBeRetried : IMessage
        {
        }
    }
}