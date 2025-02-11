namespace NServiceBus
{
    using System;
    using Transport;

    class MessageToBeRetried : MessageProcessingFailed
    {
        public int Attempt { get; }
        public TimeSpan Delay { get; }
        public bool IsImmediateRetry { get; }

        public MessageToBeRetried(int attempt, TimeSpan delay, bool immediateRetry, IncomingMessage failedMessage, Exception exception) : base(failedMessage, exception)
        {
            Attempt = attempt;
            Delay = delay;
            IsImmediateRetry = immediateRetry;
        }
    }
}