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
    public class ChangeFeedProcessor
    {
        /// <summary>
        /// Name of the Event Hub.
        /// </summary>
        private EventHubProducerClient eventHubProducerClient;
        public ChangeFeedProcessor(EventHubProducerClient _eventHubProducerClient)
        {
            eventHubProducerClient = _eventHubProducerClient;
        }

        /// <summary>
        /// Processes modified records from Cosmos DB Collection into the Event Hub.
        /// </summary>
        /// <param name="documents"> Modified records from Cosmos DB collections. </param>
        /// <param name="log"> Outputs modified records to Event Hub. </param>
        [FunctionName("ChangeFeedProcessor")]
        public async Task Run(
            //change database name below if different than specified in the lab
            [CosmosDBTrigger(databaseName: "changefeedlabdatabase",
            //change the collection name below if different than specified in the lab
            collectionName: "changefeedlabcollection",
            ConnectionStringSetting = "DBconnection",
            LeaseConnectionStringSetting = "DBconnection",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> documents, ILogger log)
        {
            var batches = default(IEnumerable<EventDataBatch>);
            var eventsToSend = new Queue<EventData>();

            try
            {
                foreach (var doc in documents)
                {
                    string json = JsonSerializer.Serialize(doc);
                    EventData data = new EventData(json);
                    eventsToSend.Enqueue(data);
                }

                batches = await BuildBatchesAsync(eventsToSend, eventHubProducerClient);
                foreach (var batch in batches)
                {
                    await eventHubProducerClient.SendAsync(batch);
                    batch.Dispose();
                }
            }
            finally
            {
                foreach (EventDataBatch batch in batches ?? Array.Empty<EventDataBatch>())
                {
                    batch.Dispose();
                }

                await eventHubProducerClient.CloseAsync();
            }

            private static async Task<IReadOnlyList<EventDataBatch>> BuildBatchesAsync(

                         Queue<EventData> queuedEvents,
                         EventHubProducerClient producer)
            {
                var batches = new List<EventDataBatch>();
                var currentBatch = default(EventDataBatch);
                int index = 0;
                while (queuedEvents.Count > 0)
                {
                    index++;
                    currentBatch ??= (await producer.CreateBatchAsync().ConfigureAwait(false));
                    EventData eventData = queuedEvents.Peek();

                    if (!currentBatch.TryAdd(eventData))
                    {
                        if (currentBatch.Count == 0)
                        {
                            throw new Exception($"The event at { index } too large to fit into a batch.");
                        }

                        batches.Add(currentBatch);
                        currentBatch = default;
                    }
                    else
                    {
                        queuedEvents.Dequeue();
                    }
                }

                if ((currentBatch != default) && (currentBatch.Count > 0))
                {
                    batches.Add(currentBatch);
                }

                return batches;
            }
        }
    }
}
