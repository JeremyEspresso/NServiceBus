﻿namespace NServiceBus.AcceptanceTests.Core.Causation
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_overriding_conversation_id_generation : NServiceBusAcceptanceTest
    {
        const string TennantIdHeaderKey = "TennantId";

        [Test]
        public async Task Should_use_custom_id()
        {
            var myBusinessMessage = new MessageSentOutsideOfHandlerMatchingTheConvention
            {
                MyBusinessId = "some id"
            };

            var tennantId = "acme";

            var context = await Scenario.Define<Context>()
                .WithEndpoint<CustomGeneratorEndpoint>(b => b.When(async session =>
                {
                    var options = new SendOptions();

                    options.RouteToThisEndpoint();
                    options.SetHeader(TennantIdHeaderKey, tennantId);

                    await session.Send(myBusinessMessage, options);
                    await session.SendLocal(new MessageSentOutsideOfHandlerNotMatchingTheConvention());
                }))
                .Done(c => c.MatchingMessageReceived && c.NonMatchingMessageReceived)
                .Run();

            Assert.AreEqual($"{tennantId}-{myBusinessMessage.MyBusinessId}", context.MatchingConversationIdReceived);
            Assert.True(Guid.TryParse(context.NonMatchingConversationIdReceived, out var _));
        }

        public class Context : ScenarioContext
        {
            public string MatchingConversationIdReceived { get; set; }
            public bool MatchingMessageReceived { get; set; }
            public string NonMatchingConversationIdReceived { get; set; }
            public bool NonMatchingMessageReceived { get; set; }
        }

        public class CustomGeneratorEndpoint : EndpointConfigurationBuilder
        {
            public CustomGeneratorEndpoint()
            {
                EndpointSetup<DefaultServer>(c => c.CustomConversationIdStrategy(MyCustomConversationIdStrategy));
            }

            ConversationId MyCustomConversationIdStrategy(ConversationIdStrategyContext context)
            {
                if (context.Message.Instance is MessageSentOutsideOfHandlerMatchingTheConvention message)
                {
                    return ConversationId.Custom($"{context.Headers[TennantIdHeaderKey]}-{message.MyBusinessId}");
                }

                return ConversationId.Default;
            }

            public Context Context { get; set; }

            public class MessageSentOutsideOfHandlerMatchingTheConventionHandler : IHandleMessages<MessageSentOutsideOfHandlerMatchingTheConvention>
            {
                public MessageSentOutsideOfHandlerMatchingTheConventionHandler(Context context)
                {
                    testContext = context;
                }

                public Task Handle(MessageSentOutsideOfHandlerMatchingTheConvention message, IMessageHandlerContext context)
                {
                    testContext.MatchingConversationIdReceived = context.MessageHeaders[Headers.ConversationId];
                    testContext.MatchingMessageReceived = true;
                    return Task.CompletedTask;
                }

                Context testContext;
            }

            public class MessageSentOutsideOfHandlerNotMatchingTheConventionHandler : IHandleMessages<MessageSentOutsideOfHandlerNotMatchingTheConvention>
            {
                public MessageSentOutsideOfHandlerNotMatchingTheConventionHandler(Context context)
                {
                    testContext = context;
                }

                public Task Handle(MessageSentOutsideOfHandlerNotMatchingTheConvention message, IMessageHandlerContext context)
                {
                    testContext.NonMatchingConversationIdReceived = context.MessageHeaders[Headers.ConversationId];
                    testContext.NonMatchingMessageReceived = true;
                    return Task.CompletedTask;
                }

                Context testContext;
            }
        }

        public class MessageSentOutsideOfHandlerMatchingTheConvention : IMessage
        {
            public string MyBusinessId { get; set; }
        }

        public class MessageSentOutsideOfHandlerNotMatchingTheConvention : IMessage
        {
        }
    }
}