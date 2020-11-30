using System;

using EasyKeys.Extensions.Data.Dapper;
using EasyKeys.Extensions.Data.Dapper.Options;
using EasyKeys.Extensions.Data.Dapper.Repositories;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DataServiceCollectionExtensions
    {
        public static IServiceCollection AddDapperRepository<T>(
                   this IServiceCollection services,
                   Action<DbOptions, IConfiguration>? configure = default) where T : AuditableEntity, new()
        {
            return services.AddDapperRepository<T>(typeof(T).Name, configure);
        }

        public static IServiceCollection AddDapperRepository<T>(
            this IServiceCollection services,
            string namedOption = "",
            Action<DbOptions, IConfiguration>? configure = default) where T : AuditableEntity, new()
        {
            var sectionName = string.IsNullOrEmpty(namedOption) ? typeof(T).Name : namedOption;
            services.AddChangeTokenOptions<DbOptions>(
                sectionName: sectionName,
                configureAction: (o, c) => configure?.Invoke(o, c));

            services.TryAddScoped<ICommandExecuter, CommandExecuter>();
            services.TryAddScoped<IAsyncRepository<T>, DapperRepository<T>>();

            return services;
        }

        public static IServiceCollection AddDapperCachedRepository<T>(
            this IServiceCollection services,
            Action<DbCachedOptions, IConfiguration>? configure = default) where T : AuditableEntity, new()
        {
            return services.AddDapperCachedRepository<T>(typeof(T).Name, configure);
        }

        public static IServiceCollection AddDapperCachedRepository<T>(
            this IServiceCollection services,
            string namedOption = "",
            Action<DbCachedOptions, IConfiguration>? configure = default,
            Action<MemoryDistributedCacheOptions>? configureCache = default) where T : AuditableEntity, new()
        {
            var sectionName = string.IsNullOrEmpty(namedOption) ? typeof(T).Name : namedOption;

            services.AddChangeTokenOptions<DbCachedOptions>(
                sectionName: sectionName,
                configureAction: (o, c) => configure?.Invoke(o, c));

            services.TryAddScoped<ICommandExecuter, CommandExecuter>();
            services.TryAddScoped<IAsyncRepositoryCache<T>, DapperRepositoryCache<T>>();

            if (configureCache != null)
            {
                services.AddDistributedMemoryCache(configureCache);
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            return services;
        }
    }
}
