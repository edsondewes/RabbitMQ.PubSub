﻿using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.PubSub;
using RabbitMQ.PubSub.HostedServices;
using RabbitMQ.PubSub.Serializers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add RabbitMQ PubSub essential services.
        /// </summary>
        /// <param name="services">The Microsoft.Extensions.DependencyInjection.IServiceCollection to register with.</param>
        /// <returns>The original Microsoft.Extensions.DependencyInjection.IServiceCollection.</returns>
        public static IServiceCollection AddRabbitPubSub(this IServiceCollection services)
        {
            MessagePack.MessagePackSerializer.SetDefaultResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);

            services.AddSingleton<IConnectionHelper, ConnectionHelper>();
            services.AddSingleton<IMessageConsumer, MessageConsumer>();
            services.AddSingleton<IMessageProducer, MessageProducer>();
            services.AddSingleton<ISerializationManager, SerializationManagerImpl>();
            services.AddSingleton<ISerializer, MessagePackSerializer>();

            return services;
        }

        /// <summary>
        /// Add a Action Flow RabbitMQ PubSub consumer hosted service.
        /// The service will be added as a Singleton.
        /// </summary>
        /// <typeparam name="TObj">Type of the serialized object</typeparam>
        /// <typeparam name="TService">Type of the processing service</typeparam>
        /// <param name="services">The Microsoft.Extensions.DependencyInjection.IServiceCollection to register with.</param>
        /// <returns>The original Microsoft.Extensions.DependencyInjection.IServiceCollection.</returns>
        public static IServiceCollection AddActionFlowConsumer<TObj, TService>(this IServiceCollection services)
            where TService : class, IBackgroundConsumer<TObj>
        {
            services.TryAddSingleton<TService>();
            services.AddHostedService<ActionFlowConsumerService<TObj, TService>>();

            return services;
        }
    }
}
