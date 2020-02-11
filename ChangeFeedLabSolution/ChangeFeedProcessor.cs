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
    using System.Collections.Generic;
    using System.Configuration;
    using System.Text;
    using Azure.Messaging.EventHubs;
    using Azure.Messaging.EventHubs.Producer;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Newtonsoft.Json;

    /// <summary>
    /// Processes events using Cosmos DB Change Feed.
    /// </summary>
    public static class ChangeFeedProcessor
    {
        /// <summary>
        /// Name of the Event Hub.
        /// </summary>
        private static readonly string EventHubName = "event-hub1";

        /// <summary>
        /// Processes modified records from Cosmos DB Collection into the Event Hub.
        /// </summary>
        /// <param name="documents"> Modified records from Cosmos DB collections. </param>
        /// <param name="log"> Outputs modified records to Event Hub. </param>
        [FunctionName("ChangeFeedProcessor")]
        public static async void Run(
            //change database name below if different than specified in the lab
            [CosmosDBTrigger(databaseName: "changefeedlabdatabase",
            //change the collection name below if different than specified in the lab
            collectionName: "changefeedlabcollection",
            ConnectionStringSetting = "DBconnection",
            LeaseConnectionStringSetting = "DBconnection",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> documents, TraceWriter log)
        {
            // Create variable to hold connection string to enable event hub namespace access.
#pragma warning disable CS0618 // Type or member is obsolete
            string eventHubNamespaceConnection = ConfigurationSettings.AppSettings["EventHubNamespaceConnection"];
#pragma warning restore CS0618 // Type or member is obsolete

            // Create producer client to send change feed events to event hub.
            await using (var producer = new EventHubProducerClient(eventHubNamespaceConnection, EventHubName))
            {
                using EventDataBatch eventBatch = await producer.CreateBatchAsync();
                // Iterate through modified documents from change feed.
                foreach (var doc in documents)
                {
                    // Convert documents to json.
                    string json = JsonConvert.SerializeObject(doc);
                    EventData data = new EventData(Encoding.UTF8.GetBytes(json));
                    eventBatch.TryAdd(data);
                }
                // Use producer  client to send the change events to event hub.
                await producer.SendAsync(eventBatch);
            }
        }
    }
}
