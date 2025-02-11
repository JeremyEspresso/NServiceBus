﻿namespace NServiceBus.AcceptanceTests.Recoverability
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Transactions;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_message_is_moved_to_error_queue_using_dtc : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_not_commit_distributed_transaction()
        {
            Requires.DtcSupport();

            var context = await Scenario.Define<Context>(c => c.Id = Guid.NewGuid())
                .WithEndpoint<Endpoint>(b => b.DoNotFailOnErrorMessages()
                    .When((session, ctx) => session.SendLocal(new MessageToFail
                    {
                        Id = ctx.Id
                    }))
                )
                .WithEndpoint<ErrorSpy>()
                .Done(c => c.MessageMovedToErrorQueue)
                .Run();

            Assert.That(context.TransactionStatuses, Is.All.Not.EqualTo(TransactionStatus.Committed));
        }

        class Context : ScenarioContext
        {
            public Guid Id { get; set; }
            public bool MessageMovedToErrorQueue { get; set; }
            public List<TransactionStatus> TransactionStatuses { get; } = new List<TransactionStatus>();
        }

        class Endpoint : EndpointConfigurationBuilder
        {
            public Endpoint()
            {
                EndpointSetup<DefaultServer>(config =>
                {
                    config.SendFailedMessagesTo(Conventions.EndpointNamingConvention(typeof(ErrorSpy)));
                });
            }

            class FailingHandler : IHandleMessages<MessageToFail>
            {
                public FailingHandler(Context context)
                {
                    testContext = context;
                }

                public Task Handle(MessageToFail message, IMessageHandlerContext context)
                {
                    if (message.Id == testContext.Id)
                    {
                        Transaction.Current.TransactionCompleted += CaptureTransactionStatus;
                    }

                    throw new SimulatedException();
                }

                void CaptureTransactionStatus(object sender, TransactionEventArgs args)
                {
                    testContext.TransactionStatuses.Add(args.Transaction.TransactionInformation.Status);
                }

                Context testContext;
            }
        }

        class ErrorSpy : EndpointConfigurationBuilder
        {
            public ErrorSpy()
            {
                EndpointSetup<DefaultServer>();
            }

            class Handler : IHandleMessages<MessageToFail>
            {
                public Handler(Context context)
                {
                    testContext = context;
                }

                public Task Handle(MessageToFail message, IMessageHandlerContext context)
                {
                    if (message.Id == testContext.Id)
                    {
                        testContext.MessageMovedToErrorQueue = true;
                    }

                    return Task.CompletedTask;
                }

                Context testContext;
            }
        }

        public class MessageToFail : IMessage
        {
            public Guid Id { get; set; }
        }
    }
}