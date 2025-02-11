﻿namespace NServiceBus.Core.Tests.Pipeline
{
    using System;
    using NServiceBus.Pipeline;
    using System.Threading.Tasks;
    using MessageInterfaces.MessageMapper.Reflection;

    class TestableMessageOperations : MessageOperations
    {
        public Pipeline<IOutgoingPublishContext> PublishPipeline => (Pipeline<IOutgoingPublishContext>)publishPipeline;
        public Pipeline<IOutgoingSendContext> SendPipeline => (Pipeline<IOutgoingSendContext>)sendPipeline;
        public Pipeline<IOutgoingReplyContext> ReplyPipeline => (Pipeline<IOutgoingReplyContext>)replyPipeline;
        public Pipeline<ISubscribeContext> SubscribePipeline => (Pipeline<ISubscribeContext>)subscribePipeline;
        public Pipeline<IUnsubscribeContext> UnsubscribePipeline => (Pipeline<IUnsubscribeContext>)unsubscribePipeline;

        public TestableMessageOperations() : base(new MessageMapper(), new Pipeline<IOutgoingPublishContext>(), new Pipeline<IOutgoingSendContext>(), new Pipeline<IOutgoingReplyContext>(), new Pipeline<ISubscribeContext>(), new Pipeline<IUnsubscribeContext>(), new ActivityFactory())
        {
        }

        public class Pipeline<TContext> : IPipeline<TContext> where TContext : IBehaviorContext
        {
            public Action<TContext> OnInvoke { get; set; }

            public TContext LastContext { get; private set; }

            public Task Invoke(TContext context)
            {
                LastContext = context;
                OnInvoke?.Invoke(context);

                return Task.CompletedTask;
            }
        }
    }
}