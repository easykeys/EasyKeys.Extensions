using System;

using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyKeys.Extensions.Storage.Azure
{
    public class CloudTableClientFactory
    {
        private readonly ILogger<CloudTableClientFactory> _logger;
        private AzureStorageOptions _options;

        public CloudTableClientFactory(
            IOptionsMonitor<AzureStorageOptions> optionsMonitor,
            ILogger<CloudTableClientFactory> logger)
        {
            _options = optionsMonitor.CurrentValue;

            optionsMonitor.OnChange(x => _options = x);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public CloudTableClient Create()
        {
            var storageAccount = CreateStorageAccountFromConnectionString(_options.ConnectionString);

            if (storageAccount == null)
            {
                throw new NullReferenceException($"{nameof(storageAccount)} wasn't created please make sure Connection String is provided.");
            }

            return storageAccount.CreateCloudTableClient(new TableClientConfiguration());
        }

        private CloudStorageAccount? CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            try
            {
                if (CloudStorageAccount.TryParse(storageConnectionString, out var storageAccount))
                {
                    return storageAccount;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex.Message);
            }

            return null;
        }
    }
}
