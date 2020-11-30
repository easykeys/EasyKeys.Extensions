using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace QueueWorkerSample
{
    public class Program
    {
        public static string QueueName = "sendgridwebhook_dev";

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                        .ConfigureAppConfiguration((hostingContext, configBuilder) =>
                        {
                            // based on environment Development = dev; Production = prod prefix in Azure Vault.
                            var envName = hostingContext.HostingEnvironment.EnvironmentName;

                            var configuration = configBuilder.AddAzureKeyVault(
                                hostingEnviromentName: envName,
                                usePrefix: true,
                                reloadInterval: TimeSpan.FromSeconds(30));

                            // helpful to see what was retrieved from all of the configuration providers.
                            if (hostingContext.HostingEnvironment.IsDevelopment())
                            {
                                // var configuration = configBuilder.Build();
                                configuration.DebugConfigurations();
                            }
                        })
                        .ConfigureServices((hostContext, services) =>
                        {
                            services.AddAzureServiceBus(Program.QueueName);
                            services.AddScoped<SendGridQueueEventHandler>();
                            services.AddHostedService<Worker>();
                        });
        }
    }
}
