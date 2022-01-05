using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(ChangeFeedFunction.Startup))]
namespace ChangeFeedFunction
{
    public class Startup : FunctionsStartup
    {
        public class MyOptions
        {
            public string EventHubNamespaceConnection { get; set; }
            public string EventHubName { get; set; }
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<MyOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("MyOptions").Bind(settings);
                });
        }
    }
}
