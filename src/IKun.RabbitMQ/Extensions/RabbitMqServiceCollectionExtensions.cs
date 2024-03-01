namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// RabbitMQ Extensions
    /// </summary>
    public static class RabbitMqServiceCollectionExtensions
    {
        /// <summary>
        /// 添加RabbitMq
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TRequestProcessor"></typeparam>
        /// <param name="services"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddRabbitMq<TRequest, TRequestProcessor>(
            this IServiceCollection services,
            Action<RabbitMqOptions> action)
            where TRequest : IRabbitMqRequest
            where TRequestProcessor : IRequestProcessorHandler<TRequest>
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var rabbitMqOptions = new RabbitMqOptions();
            action(rabbitMqOptions);

            services.AddRabbitMq<TRequest, TRequestProcessor>(rabbitMqOptions);

            return services;
        }

        /// <summary>
        /// 添加RabbitMq
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TRequestProcessor"></typeparam>
        /// <param name="services"></param>
        /// <param name="rabbitMqOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMq<TRequest, TRequestProcessor>(
            this IServiceCollection services,
            IRabbitMqOptions rabbitMqOptions)
            where TRequest : IRabbitMqRequest
            where TRequestProcessor : IRequestProcessorHandler<TRequest>
        {
            var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();

            services.TryAddSingleton<IRabbitMqConnectionProxy>(_ => new RabbitMqConnectionProxy(rabbitMqOptions, loggerFactory.CreateLogger<RabbitMqConnectionProxy>()));

            services.TryAddSingleton<IRabbitMqProducer<TRequest>>(serviceProvider => new RabbitMqProducer<TRequest>(
                    serviceProvider.GetRequiredService<IRabbitMqConnectionProxy>(),
                    loggerFactory.CreateLogger<RabbitMqProducer<TRequest>>()
                    )
            );

            services.TryAddSingleton<IRequestConsumer<TRequest>, RequestCosumer<TRequest>>();

            services.TryAddSingleton<IRabbitMqConsumer<TRequest>>(serviceProvider => new RabbitMqConsumer<TRequest>(
                    serviceProvider.GetRequiredService<IRabbitMqConnectionProxy>(),
                    loggerFactory.CreateLogger<RabbitMqConsumer<TRequest>>()
                    )
            );

            services.TryAddSingleton(typeof(IRequestProcessorHandler<TRequest>), typeof(TRequestProcessor));

            services.TryAddSingleton<IConsumerProcessorManager<TRequest>>(serviceProvider => new ConsumerProcessorManager<TRequest>(
                    serviceProvider.GetRequiredService<IRequestConsumer<TRequest>>(),
                    serviceProvider.GetRequiredService<IRequestProcessorHandler<TRequest>>(),
                    loggerFactory.CreateLogger<ConsumerProcessorManager<TRequest>>()
                    )
            );

            return services;
        }
    }
}