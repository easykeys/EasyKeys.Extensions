using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace EasyKeys.Extensions.Storage.Azure
{
    public interface ICloudBlobClientFactory
    {
        CloudStorageAccount CreateAccount();

        CloudBlobClient CreateClient();
    }
}
