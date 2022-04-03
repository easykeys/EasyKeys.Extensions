using EasyKeys.Extensions.Storage.Abstractions;

namespace EasyKeys.Extensions.Storage.Azure;

public interface IStorageFactory
{
    /// <summary>
    /// Name of the registed <see cref="IStorageFactory"/>.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Options for the storage.
    /// </summary>
    AzureStorageOptions Options { get; }

    /// <summary>
    /// Instance of the Blob Storage.
    /// </summary>
    /// <returns></returns>
    IBlobContainer GetContainer();
}
