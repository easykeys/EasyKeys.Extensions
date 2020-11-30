using System;

using EasyKeys.Extensions.Storage.Abstractions;
using EasyKeys.Extensions.Storage.Azure;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureBlobStorageServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Azure Blob Storage.
        /// </summary>
        /// <param name="services">The DI services.</param>
        /// <param name="sectionName">The section name for <see cref="AzureStorageOptions"/>.</param>
        /// <param name="configureOptions">The override configuration for <see cref="AzureStorageOptions"/>.</param>
        /// <returns></returns>
        public static IServiceCollection AddAzureBlobStorage(
            this IServiceCollection services,
            string sectionName = "StorageProviders:AzureStorage",
            Action<AzureStorageOptions, IServiceProvider>? configureOptions = default)
        {
            services
                .AddChangeTokenOptions<AzureStorageOptions>(
                  sectionName: sectionName,
                  configureAction: (opt, sp) => configureOptions?.Invoke(opt, sp));

            services.AddScoped<IBlobStorage, BlobStorage>();
            services.AddScoped<ICloudBlobClientFactory, CloudBlobClientFactory>();

            return services;
        }
    }
}
