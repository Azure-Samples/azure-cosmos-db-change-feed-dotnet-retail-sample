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
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.EventHubs;
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
        private static readonly string EventHubName = "eventhub";

        /// <summary>
        /// Processes modified records from Cosmos DB Collection into the Event Hub.
        /// </summary>
        /// <param name="documents"> Modified records from Cosmos DB collections. </param>
        /// <param name="log"> Outputs modified records to Event Hub. </param>
        [FunctionName("ChangeFeedProcessor")]
        public static void Run(
            [CosmosDBTrigger(databaseName: "database1",
            collectionName: "YOUR MAIN COLLECTION NAME HERE",
            ConnectionStringSetting = "DBconnection",
            LeaseCollectionName = "YOUR LEASES COLLECTION NAME HERE")]IReadOnlyList<Document> documents, TraceWriter log)
        {
            // Create variable to hold connection string to enable event hub namespace access.
#pragma warning disable CS0618 // Type or member is obsolete
            string eventHubNamespaceConnection = ConfigurationSettings.AppSettings["EventHubNamespaceConnection"];
#pragma warning restore CS0618 // Type or member is obsolete

            // Build connection string to access event hub within event hub namespace.
            EventHubsConnectionStringBuilder eventHubConnectionStringBuilder = 
                new EventHubsConnectionStringBuilder(eventHubNamespaceConnection)
            {
                EntityPath = EventHubName
            };

            // Create event hub client to send change feed events to event hub.
            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(eventHubConnectionStringBuilder.ToString());

            // Iterate through modified documents from change feed.
            foreach (var doc in documents)
            {
                // Convert documents to json.
                string json = JsonConvert.SerializeObject(doc);
                EventData data = new EventData(Encoding.UTF8.GetBytes(json));

                // Use Event Hub client to send the change events to event hub.
                eventHubClient.SendAsync(data);
            }
        }
    }
}
