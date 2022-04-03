using EasyKeys.Extensions.Storage.Abstractions;

namespace EasyKeys.Extensions.Storage.Azure;

public class AzureStorageOptions : BlobStorageOptions
{
    public string AccountName { get; set; } = string.Empty;

    public string AccountKey { get; set; } = string.Empty;

    public string EndpointSuffix { get; set; } = string.Empty;

    public string ConnectionString { get; set; } = string.Empty;
}
