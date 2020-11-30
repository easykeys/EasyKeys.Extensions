using System;

using EasyKeys.Extensions.Storage.Abstractions;
using EasyKeys.Extensions.Storage.FileSystem;
using EasyKeys.Extensions.Storage.InMemory;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StorageServiceCollectionExtensions
    {
        /// <summary>
        /// Add Local File Storage to DI registration.
        /// </summary>
        public static IServiceCollection AddFileStorage(
            this IServiceCollection services,
            string sectionName = " StorageProviders:FileStorage",
            Action<FileSystemBlobStorageOptions, IServiceProvider>? configureOptions = default)
        {
            services
                .AddChangeTokenOptions<FileSystemBlobStorageOptions>(
                    sectionName: sectionName,
                    configureAction: (opt, sp) => configureOptions?.Invoke(opt, sp));

            services.AddScoped<IBlobContainer, FileSystemBlobStorage>();
            services.AddScoped<IBlobStorage, FileSystemBlobStorage>();

            return services;
        }

        /// <summary>
        /// Add Memory File Storage to DI registration.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="sectionName"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddMemoryStorage(
            this IServiceCollection services,
            string sectionName = " StorageProviders:MemoryStorage",
            Action<InMemoryBlobStorageOptions, IServiceProvider>? configureOptions = default)
        {
            services
                .AddChangeTokenOptions<InMemoryBlobStorageOptions>(
                    sectionName: sectionName,
                    configureAction: (opt, sp) => configureOptions?.Invoke(opt, sp));

            services.AddScoped<IBlobContainer, FileSystemBlobStorage>();
            services.AddScoped<IBlobStorage, FileSystemBlobStorage>();

            return services;
        }
    }
}
