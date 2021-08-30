//-----------------------------------------------------------------------
// <copyright file="ChangeFeedProcessor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved. 
// </copyright>
// <author>Serena Davis</author>
//-----------------------------------------------------------------------

/// <summary>
/// Azure Function triggered by Cosmos DB Change Feed that sends modified records to Event Hub
/// </summary>
namespace ChangeFeedFunction
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Azure.Messaging.EventHubs;
    using Azure.Messaging.EventHubs.Producer;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Processes events using Cosmos DB Change Feed.
    /// </summary>
    public static class ChangeFeedProcessor
    {
        /// <summary>
        /// Name of the Event Hub.
        /// </summary>
        private const string EventHubName = "event-hub1";

        // Create producer client to send change feed events to event hub.
        private static readonly Lazy<EventHubProducerClient> lazyClient = new Lazy<EventHubProducerClient>(InitializeEventHubProducerClient);

        private static EventHubProducerClient producer => lazyClient.Value;

        private static EventHubProducerClient InitializeEventHubProducerClient()
        {
            // Create variable to hold connection string to enable event hub namespace access.
            string eventHubNamespaceConnection = ConfigurationManager.AppSettings["EventHubNamespaceConnection"];

            return new EventHubProducerClient(eventHubNamespaceConnection, EventHubName);
        }
        /// <summary>
        /// Processes modified records from Cosmos DB Collection into the Event Hub.
        /// </summary>
        /// <param name="documents"> Modified records from Cosmos DB collections. </param>
        /// <param name="log"> Outputs modified records to Event Hub. </param>
        [FunctionName("ChangeFeedProcessor")]
        public static async Task Run(
            //change database name below if different than specified in the lab
            [CosmosDBTrigger(databaseName: "changefeedlabdatabase",
            //change the collection name below if different than specified in the lab
            collectionName: "changefeedlabcollection",
            ConnectionStringSetting = "DBconnection",
            LeaseConnectionStringSetting = "DBconnection",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> documents, ILogger log)
        {
            using EventDataBatch eventBatch = await producer.CreateBatchAsync();
            int count = 0;
            // Iterate through modified documents from change feed.
            foreach (var doc in documents)
            {
                count++;
                // Convert documents to Json.
                string json = JsonSerializer.Serialize(doc);
                EventData data = new EventData(Encoding.UTF8.GetBytes(json));
                if (!eventBatch.TryAdd(data))
                {
                    throw new Exception($"The event at doc{count} could not be added.");
                }
            }
            // Use the producer to send the change events to Event Hubs.
            await producer.SendAsync(eventBatch);
        }
    }
}
