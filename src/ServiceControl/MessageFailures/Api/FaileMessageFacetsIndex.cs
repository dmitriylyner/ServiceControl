﻿namespace ServiceControl.MessageFailures.Api
{
    using System.Linq;
    using Contracts.Operations;
    using Raven.Abstractions.Indexing;
    using Raven.Client.Indexes;

    public class FaileMessageFacetsIndex : AbstractIndexCreationTask<FailedMessage>
    {
        public FaileMessageFacetsIndex()
        {
            Map = failures => from failure in failures
                select new
                {
                    ((EndpointDetails)failure.MostRecentAttempt.MessageMetadata["ReceivingEndpoint"].Value).Name,
                    ((EndpointDetails)failure.MostRecentAttempt.MessageMetadata["ReceivingEndpoint"].Value).Machine,
                    MessageType = failure.MostRecentAttempt.MessageMetadata["MessageType"].Value
                };
            Index("Name",FieldIndexing.NotAnalyzed); //to avoid lower casing
            Index("Machine", FieldIndexing.NotAnalyzed); //to avoid lower casing
            Index("MessageType", FieldIndexing.NotAnalyzed); //to avoid lower casing
        }
    }
}