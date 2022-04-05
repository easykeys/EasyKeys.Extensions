namespace EasyKeys.Extensions.Storage.Abstractions;

public class BlobStorageOptions
{
    /// <summary>
    /// The name of the storage.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The default path.
    /// </summary>
    public string DefaultPath { get; set; } = string.Empty;
}
