namespace NServiceBus.AcceptanceTests.Causation
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_a_message_is_faulted : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_flow_causation_headers()
        {
            var context = await Scenario.Define<Context>()
                    .WithEndpoint<CausationEndpoint>(b => b.When(bus =>
                    {
                        var options = new SendOptions();
                        options.SetHeader(Headers.RelatedTo, Guid.NewGuid().ToString());
                        options.RouteToLocalEndpointInstance();
                        return bus.Send(new MessageThatFails(), options);
                        
                    }).DoNotFailOnErrorMessages())
                    .WithEndpoint<EndpointThatHandlesErrorMessages>()
                    .Done(c => c.Done)
                    .Run();

            Assert.True(context.IsRelatedToReceived, "The RelatedTo header should be included in faulted message.");
            Assert.True(context.IsConversationIdReceived, "The ConversationId header should be included in faulted message.");
        }


        public class Context : ScenarioContext
        {
            public bool Done { get; set; }
            public bool IsRelatedToReceived { get; set; }
            public bool IsConversationIdReceived { get; set; }
        }

        public class CausationEndpoint : EndpointConfigurationBuilder
        {
            public CausationEndpoint()
            {
                EndpointSetup<DefaultServer>((c, r) =>
                {
                    c.DisableFeature<Features.SecondLevelRetries>();
                    c.SendFailedMessagesTo("errorQueueForAcceptanceTest");
                });
            }

            public Context Context { get; set; }
            
            public class MessageSentInsideHandlersHandler : IHandleMessages<MessageThatFails>
            {
                public Context TestContext { get; set; }

                public Task Handle(MessageThatFails message, IMessageHandlerContext context)
                {
                    throw new SimulatedException();
                }
            }
        }

        class EndpointThatHandlesErrorMessages : EndpointConfigurationBuilder
        {

            public EndpointThatHandlesErrorMessages()
            {
                EndpointSetup<DefaultServer>()
                    .CustomEndpointName("errorQueueForAcceptanceTest");
            }

            class ErrorMessageHandler : IHandleMessages<MessageThatFails>
            {
                Context testContext;

                public ErrorMessageHandler(Context testContext)
                {
                    this.testContext = testContext;
                }

                public Task Handle(MessageThatFails message, IMessageHandlerContext context)
                {
                    testContext.IsRelatedToReceived = context.MessageHeaders.ContainsKey(Headers.RelatedTo);
                    testContext.IsConversationIdReceived = context.MessageHeaders.ContainsKey(Headers.ConversationId);
                    testContext.Done = true;

                    return Task.FromResult(0); // ignore messages from previous test runs
                }
            }
        }

        public class MessageThatFails : IMessage
        {
        }
    }
}
