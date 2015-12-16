namespace NServiceBus.AcceptanceTests.Causation
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.Pipeline;
    using NUnit.Framework;

    public class When_a_message_is_audited : NServiceBusAcceptanceTest
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
                        return bus.Send(new MessageToBeAudited(), options);
                    }))
                    .WithEndpoint<AuditSpyEndpoint>()
                    .Done(c => c.Done)
                    .Run();

            Assert.True(context.IsRelatedToReceived, "The RelatedTo header should be be included in audited message.");
            Assert.True(context.IsConversationIdReceived, "The ConversationId header should be included in audited message.");
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
                EndpointSetup<DefaultServer>().AuditTo<AuditSpyEndpoint>();
            }

            public Context Context { get; set; }
            
            public class MessageSentInsideHandlersHandler : IHandleMessages<MessageToBeAudited>
            {
                public Task Handle(MessageToBeAudited message, IMessageHandlerContext context)
                {
                    return Task.FromResult(0);
                }
            }
        }

        class AuditSpyEndpoint : EndpointConfigurationBuilder
        {
            public AuditSpyEndpoint()
            {
                EndpointSetup<DefaultServer>();
            }

            public class MessageToBeAuditedHandler : IHandleMessages<MessageToBeAudited>
            {
                public Context TestContext { get; set; }

                public Task Handle(MessageToBeAudited message, IMessageHandlerContext context)
                {
                    TestContext.IsRelatedToReceived = context.MessageHeaders.ContainsKey(Headers.RelatedTo);
                    TestContext.IsConversationIdReceived = context.MessageHeaders.ContainsKey(Headers.ConversationId);
                    TestContext.Done = true;

                    return Task.FromResult(0);
                }
            }
        }
        
        public class MessageToBeAudited : IMessage
        {
        }

    }
}
