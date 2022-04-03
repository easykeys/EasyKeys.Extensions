using EasyKeys.Extensions.Storage.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyKeys.Extensions.Storage.Azure;

public class StorageFactory : IStorageFactory
{
    private readonly IServiceProvider _sp;

    public StorageFactory(
        string name,
        IServiceProvider sp)
    {
        Name = name;
        _sp = sp;
        Options = _sp.GetRequiredService<IOptionsMonitor<AzureStorageOptions>>().Get(Name);
    }

    public string Name { get; }

    public AzureStorageOptions Options { get; }

    public IBlobContainer GetContainer()
    {
        var azureLogger = _sp.GetRequiredService<ILogger<CloudBlobClientFactory>>();
        var factory = new CloudBlobClientFactory(Microsoft.Extensions.Options.Options.Create(Options), azureLogger);

        return new BlobStorage(factory);
    }
}
