﻿namespace NServiceBus.Unicast.Tests
{
    using System.Threading.Tasks;
    using Outbox;
    using NServiceBus.Transport;
    using NUnit.Framework;
    using Testing;
    using Core.Tests.Fakes;

    [TestFixture]
    public class LoadHandlersConnectorTests
    {
        [Test]
        public void Should_throw_when_there_are_no_registered_message_handlers()
        {
            var behavior = new LoadHandlersConnector(new MessageHandlerRegistry());

            var context = new TestableIncomingLogicalMessageContext();

            context.Extensions.Set<IOutboxTransaction>(new FakeOutboxTransaction());
            context.Extensions.Set(new TransportTransaction());

            Assert.That(async () => await behavior.Invoke(context, c => Task.CompletedTask), Throws.InvalidOperationException);
        }
    }
}