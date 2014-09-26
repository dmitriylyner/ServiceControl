namespace ServiceControl.MessageAuditing.Handlers
{
	using System;
	using Contracts.Operations;
    using NServiceBus;
    using Raven.Client;

    class AuditMessageHandler : IHandleMessages<ImportSuccessfullyProcessedMessage>
    {
        public IDocumentSession Session { get; set; }

        public void Handle(ImportSuccessfullyProcessedMessage message)
        {
            var auditMessage = new ProcessedMessage(message);
			Console.WriteLine("I am about to save a message to RavenDB");
            Session.Store(auditMessage);
        }
    }
}
