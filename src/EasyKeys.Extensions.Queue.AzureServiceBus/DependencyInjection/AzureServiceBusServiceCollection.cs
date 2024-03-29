﻿using EasyKeys.Extensions.Queue.Abstractions;
using EasyKeys.Extensions.Queue.AzureServiceBus;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureServiceBusServiceCollection
    {
        public static IServiceCollection AddAzureServiceBus(
            this IServiceCollection services,
            string queueName,
            string sectionName = "Queues:AzureServiceBus",
            Action<AzureServiceBusOptions, IServiceProvider>? configureOptions = default)
        {
            services.AddChangeTokenOptions<AzureServiceBusOptions>(
                optionName: queueName,
                sectionName: sectionName,
                configureAction: (opt, sp) =>
                {
                    configureOptions?.Invoke(opt, sp);
                    if (queueName != opt.QueueName)
                    {
                        opt.QueueName = queueName;
                    }
                });

            services.Configure<AzureServiceBusOptions>().PostConfigure<AzureServiceBusOptions>(name: queueName, configureOptions: o => o.Build());

            services.TryAddSingleton<IAzureServiceBusConnection, AzureServiceBusConnection>();
            services.TryAddSingleton<IQueueBusSubscriptionsManager, InMemoryQueueBusSubscriptionsManager>();
            services.TryAddSingleton<IQueueBus, AzureServiceBus>();

            return services;
        }
    }
}
