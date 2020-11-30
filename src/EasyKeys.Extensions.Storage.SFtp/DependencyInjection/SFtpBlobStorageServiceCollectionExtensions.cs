using System;

using EasyKeys.Extensions.Storage.Abstractions;
using EasyKeys.Extensions.Storage.SFtp;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SFtpBlobStorageServiceCollectionExtensions
    {
        /// <summary>
        /// Adds SFtp Storage Provider to DI Services.
        /// </summary>
        /// <param name="services">The DI services.</param>
        /// <param name="sectionName">The section name for options configuration.</param>
        /// <param name="configureOptions">The configurations override.</param>
        /// <returns></returns>
        public static IServiceCollection AddSFtpStorage(
            this IServiceCollection services,
            string sectionName = "StorageProviders:SFtpStorage",
            Action<SFtpBlobStorageOptions, IServiceProvider>? configureOptions = default)
        {
            services.AddChangeTokenOptions<SFtpBlobStorageOptions>(
                sectionName: sectionName,
                configureAction: (opt, sp) => configureOptions?.Invoke(opt, sp));

            services.AddScoped<IBlobStorage, SFtpBlobStorage>();

            return services;
        }
    }
}
