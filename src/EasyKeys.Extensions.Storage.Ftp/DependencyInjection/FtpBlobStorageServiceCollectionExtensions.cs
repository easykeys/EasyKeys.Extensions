using System;

using EasyKeys.Extensions.Storage.Abstractions;
using EasyKeys.Extensions.Storage.Ftp;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FtpBlobStorageServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Ftp File Storage Provider to DI services.
        /// </summary>
        /// <param name="services">The DI Services</param>
        /// <param name="sectionName"></param>
        /// <param name="configureOptions">The configuration override for <see cref="FtpBlobStorageOptions"/>.</param>
        /// <returns></returns>
        public static IServiceCollection AddFtpStorage(
            this IServiceCollection services,
            string sectionName = "StorageProviders:FtpStorage",
            Action<FtpBlobStorageOptions, IServiceProvider>? configureOptions = default)
        {
            services.AddChangeTokenOptions<FtpBlobStorageOptions>(
                sectionName: sectionName,
                configureAction: (opt, sp) => configureOptions?.Invoke(opt, sp));

            services.AddScoped<IBlobStorage, FtpBlobStorage>();

            return services;
        }
    }
}
