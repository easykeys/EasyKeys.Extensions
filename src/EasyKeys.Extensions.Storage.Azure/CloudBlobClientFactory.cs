using System;

using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyKeys.Extensions.Storage.Azure
{
    public class CloudBlobClientFactory : ICloudBlobClientFactory
    {
        private readonly ILogger<CloudBlobClientFactory> _logger;
        private AzureStorageOptions _options;

        public CloudBlobClientFactory(
            IOptions<AzureStorageOptions> options,
            ILogger<CloudBlobClientFactory> logger)
        {
            _options = options.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public CloudBlobClient CreateClient()
        {
            if (string.IsNullOrEmpty(_options.AccountName)
                || string.IsNullOrEmpty(_options.AccountKey))
            {
                if (CloudStorageAccount.TryParse(_options.ConnectionString, out var cloudStorageAccount))
                {
                    return cloudStorageAccount.CreateCloudBlobClient();
                }
            }

            var credentials = new StorageCredentials(_options.AccountName, _options.AccountKey);
            var account = new CloudStorageAccount(credentials, _options.EndpointSuffix, useHttps: true);

            _logger.LogInformation("[{name}] Created", nameof(CloudBlobClient));

            return account.CreateCloudBlobClient();
        }

        public CloudStorageAccount CreateAccount()
        {
            if (string.IsNullOrEmpty(_options.AccountName)
                || string.IsNullOrEmpty(_options.AccountKey))
            {
                if (CloudStorageAccount.TryParse(_options.ConnectionString, out var cloudStorageAccount))
                {
                    return cloudStorageAccount;
                }
            }

            var credentials = new StorageCredentials(_options.AccountName, _options.AccountKey);
            var account = new CloudStorageAccount(credentials, _options.EndpointSuffix, useHttps: true);

            _logger.LogInformation("[{name}] Created", nameof(CloudStorageAccount));

            return account;
        }
    }
}
