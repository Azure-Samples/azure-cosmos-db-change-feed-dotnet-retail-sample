using Azure.Messaging.EventHubs.Producer;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;

[assembly: FunctionsStartup(typeof(ChangeFeedFunction.Startup))]
namespace ChangeFeedFunction
{
    public class Startup : FunctionsStartup
    {
        public const string EventHubName = "event-hub1";
        public const string EventHubNamespaceConnection = "<< CONNECTION STRING FOR THE EVENT HUBS NAMESPACE >> ";
        
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddAzureClients(builder =>
            {
                var producer = new EventHubProducerClient(EventHubNamespaceConnection, EventHubName);
                builder.AddEventHubProducerClient(EventHubNamespaceConnection, EventHubName);
            });
        }
    }
}
