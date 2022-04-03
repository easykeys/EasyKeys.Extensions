using System;

using EasyKeys.Extensions.Storage.Abstractions;
using EasyKeys.Extensions.Storage.Azure;

using Microsoft.Extensions.Configuration;

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

        public static IServiceCollection AddAzureFactoryStorage(
            this IServiceCollection services,
            string name,
            string sectionName = nameof(AzureStorageOptions),
            Action<AzureStorageOptions, IConfiguration>? configure = default)
        {
            // azure storage configuration for images retrieval
            services.AddChangeTokenOptions<AzureStorageOptions>(
                sectionName: sectionName,
                optionName: name,
                configureAction: (o, c) => configure?.Invoke(o, c));

            // allows for multipe registrations with different names only..
            services.AddScoped<IStorageFactory>(sp =>
            {
                return new StorageFactory(name, sp);
            });

            return services;
        }
    }
}
