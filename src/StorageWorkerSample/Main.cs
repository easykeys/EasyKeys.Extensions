using EasyKeys.Extensions.Storage.Abstractions;
using EasyKeys.Extensions.Storage.Azure;

public class Main : IMain
{
    private readonly ILogger<Main> _logger;
    private readonly IStorageFactory _storage;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public Main(
        IEnumerable<IStorageFactory> storages,
        IHostApplicationLifetime applicationLifetime,
        IConfiguration configuration,
        ILogger<Main> logger)
    {
        _storage = storages.First(x => x.Name == "TestStorage");

        _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IConfiguration Configuration { get; set; }

    public async Task<int> RunAsync()
    {
        _logger.LogInformation("Main executed");

        // use this token for stopping the services
        var cancellationToken = _applicationLifetime.ApplicationStopping;

        var container = _storage.GetContainer();

        var path = $"{_storage.Options.DefaultPath}/test-email.txt";
        var file = await container.GetAsync(path, cancellationToken);

        using var stream = await container.OpenReadAsync(path, cancellationToken);
        File.WriteAllBytes("downloaded.txt", await stream.ToByteArrayAsync());
        return 0;
    }
}
