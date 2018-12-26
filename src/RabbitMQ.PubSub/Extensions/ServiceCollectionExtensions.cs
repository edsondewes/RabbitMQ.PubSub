using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.PubSub;
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
        /// Add an Async RabbitMQ PubSub consumer hosted service.
        /// The service will be added as Transient.
        /// </summary>
        /// <typeparam name="TObj">Type of the serialized object</typeparam>
        /// <typeparam name="TService">Type of the processing service</typeparam>
        /// <param name="services">The Microsoft.Extensions.DependencyInjection.IServiceCollection to register with.</param>
        /// <param name="builder">An action to configure the subscription behavior.</param>
        /// <returns>The original Microsoft.Extensions.DependencyInjection.IServiceCollection.</returns>
        public static IServiceCollection AddAsyncConsumer<TObj, TService>(this IServiceCollection services, Action<IAsyncConsumerOptionsBuilder<TObj>> builder = null)
            where TService : class, IBackgroundConsumer<TObj>
        {
            services.AddTransient<IHostedService>(provider =>
            {
                var optionsBuilder = new AsyncConsumerOptionsBuilder<TObj>(provider);
                builder?.Invoke(optionsBuilder);

                return new AsyncConsumerService<TObj, TService>(
                    provider.GetRequiredService<IMessageConsumer>(),
                    ActivatorUtilities.GetServiceOrCreateInstance<TService>(provider),
                    optionsBuilder.Options,
                    provider.GetRequiredService<ILogger<AsyncConsumerService<TObj, TService>>>()
                    );
            });

            return services;
        }
    }
}
