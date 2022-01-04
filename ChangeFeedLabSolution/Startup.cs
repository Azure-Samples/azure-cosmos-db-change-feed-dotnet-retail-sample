using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using System;

[assembly: FunctionsStartup(typeof(ChangeFeedFunction.Startup))]
namespace ChangeFeedFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddAzureClients(builder =>
            {
                builder.AddEventHubProducerClient(Environment.GetEnvironmentVariable("EventHubNamespaceConnection"), Environment.GetEnvironmentVariable("EventHubName"));
            });
        }
    }
}
