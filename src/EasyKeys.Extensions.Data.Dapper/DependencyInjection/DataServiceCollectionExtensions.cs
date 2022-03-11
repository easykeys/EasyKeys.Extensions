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
        /// <summary>
        /// Adds <see cref="ICommandExecuter"/> with specified connection configurations.
        /// </summary>
        /// <param name="services">The DI services.</param>
        /// <param name="sectionName">The section name to be used for connection. The default is 'ConnectionStrings'.</param>
        /// <param name="optionName">The name of the option. The default is ''.</param>
        /// <param name="configure">The configuration override.</param>
        /// <returns></returns>
        public static IServiceCollection AddDbConnection(
            this IServiceCollection services,
            string sectionName = "ConnectionStrings:ConnectionString",
            string optionName = "",
            Action<DbOptions, IConfiguration>? configure = default)
        {
            // command executer
            services.TryAddScoped<ICommandExecuter, CommandExecuter>();

            services.AddChangeTokenOptions<DbOptions>(
               sectionName: sectionName,
               optionName: optionName,
               configureAction: (o, c) => configure?.Invoke(o, c));

            return services;
        }

        /// <summary>
        /// Adds Dapper Generic Repository, the name of the entity type is going to be used for the connection string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddDapperRepository<T>(
                   this IServiceCollection services,
                   Action<DbOptions, IConfiguration>? configure = default) where T : BaseEntity, new()
        {
            return services.AddDapperRepository<T>(namedOption: typeof(T).Name, configure: configure);
        }

        public static IServiceCollection AddDapperRepository<T>(
            this IServiceCollection services,
            string sectionName = "ConnectionStrings:ConnectionString",
            string namedOption = "",
            Action<DbOptions, IConfiguration>? configure = default) where T : BaseEntity, new()
        {
            var optionName = string.IsNullOrEmpty(namedOption) ? typeof(T).Name : namedOption;
            services.AddDbConnection(sectionName, optionName, configure);

            services.TryAddScoped<IAsyncRepository<T>, DapperRepository<T>>();
            return services;
        }

        public static IServiceCollection AddDapperCachedRepository<T>(
            this IServiceCollection services,
            Action<DbOptions, IConfiguration>? configure) where T : BaseEntity, new()
        {
            return services.AddDapperCachedRepository<T>(namedOption: typeof(T).Name, configure: configure);
        }

        /// <summary>
        /// Add Dapper Cached Repository.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="sectionName"></param>
        /// <param name="namedOption"></param>
        /// <param name="configure"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddDapperCachedRepository<T>(
            this IServiceCollection services,
            string sectionName = "ConnectionStrings:ConnectionString",
            string namedOption = "",
            Action<DbOptions, IConfiguration>? configure = default,
            Action<MemoryDistributedCacheOptions>? setupAction = default) where T : BaseEntity, new()
        {
            var optionName = string.IsNullOrEmpty(namedOption) ? typeof(T).Name : namedOption;

            configure ??= (x, y) => x.ConnectionString = y[sectionName];

            services.AddChangeTokenOptions<DbOptions>(
                    sectionName: sectionName,
                    optionName: optionName,
                    configureAction: (o, c) => configure?.Invoke(o, c));

            services.TryAddScoped<IAsyncRepositoryCache<T>, DapperRepositoryCache<T>>();

            if (setupAction != null)
            {
                services.AddDistributedMemoryCache(setupAction);
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            services.TryAddScoped<ICommandExecuter, CommandExecuter>();
            return services;
        }
    }
}
