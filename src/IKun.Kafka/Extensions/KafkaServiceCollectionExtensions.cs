namespace Microsoft.Extensions.DependencyInjection;

public static class KafkaServiceCollectionExtensions
{
    /// <summary>
    /// 添加kafka服务配置
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TRequestProcessor"></typeparam>
    /// <param name="services"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IServiceCollection AddKafka<TRequest, TRequestProcessor>(
        this IServiceCollection services,
        Action<KafkaOptions> action)
        where TRequest : IKafkaRequest
        where TRequestProcessor : IRequestProcessorHandler<TRequest>
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var options = new KafkaOptions();
        action(options);

        services.AddKafka<TRequest, TRequestProcessor>(
            options
        );

        return services;
    }

    /// <summary>
    /// 添加kafka服务配置
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TRequestProcessor"></typeparam>
    /// <param name="services"></param>
    /// <param name="kafkaOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddKafka<TRequest, TRequestProcessor>(
        this IServiceCollection services,
        KafkaOptions kafkaOptions)
        where TRequest : IKafkaRequest
        where TRequestProcessor : IRequestProcessorHandler<TRequest>
    {
        var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();

        services.TryAddSingleton<IKafkaProducer<TRequest>>(_ => new KafkaProducer<TRequest>(kafkaOptions, loggerFactory.CreateLogger<KafkaProducer<TRequest>>()));
        services.TryAddSingleton<IKafkaConsumer<TRequest>>(_ => new KafkaConsumer<TRequest>(kafkaOptions, loggerFactory.CreateLogger<KafkaConsumer<TRequest>>()));
        services.TryAddSingleton<IRequestConsumer<TRequest>, RequestConsumer<TRequest>>();
        services.TryAddSingleton(typeof(IRequestProcessorHandler<TRequest>), typeof(TRequestProcessor));
        services.TryAddSingleton<IConsumerProcessorManager<TRequest>>(serviceProvider => new ConsumerProcessorManager<TRequest>(
            serviceProvider.GetRequiredService<IRequestConsumer<TRequest>>(),
            serviceProvider.GetRequiredService<IRequestProcessorHandler<TRequest>>(),
            loggerFactory.CreateLogger<ConsumerProcessorManager<TRequest>>(),
            kafkaOptions.Topic,
            kafkaOptions.MaxRequestRetryCount
            )
        );

        return services;
    }
}