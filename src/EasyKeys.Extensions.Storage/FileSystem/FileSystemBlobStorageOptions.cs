using EasyKeys.Extensions.Storage.Abstractions;

namespace EasyKeys.Extensions.Storage.FileSystem
{
    public class FileSystemBlobStorageOptions : BlobStorageOptions
    {
        public string BasePath { get; set; } = string.Empty;
    }
}
