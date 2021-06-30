using System;

using DataWorkerSample.Entities;
using DataWorkerSample.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using var host = CreateHostBuilder(args).Build();

await host.StartAsync();

using var scope = host.Services.CreateScope();

var sp = scope.ServiceProvider;

var s = sp.GetRequiredService<EmailService>();

var result = await s.GetAsync();

var emailLog = new EmailLogEntity
{
    BCCEmailList = "test@t.com",
    Body = $"Test Body{Guid.NewGuid()}",
    Code = Guid.NewGuid().ToString(),
    EmailRequestID = 0,
    FromEmail = "donotreply@t.com"
};

var id = await s.InsertAsync(emailLog);

await host.StopAsync();

static IHostBuilder CreateHostBuilder(string[] args)
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
                    services.AddDapperRepository<EmailLogEntity>(sectionName: "ConnectionStrings:Main");
                    services.AddScoped<EmailService>();
                });
}
