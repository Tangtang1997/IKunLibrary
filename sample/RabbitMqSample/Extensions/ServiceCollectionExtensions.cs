namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHostedService<SampleHostedService>();

        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

        var hostName = configuration["RabbitMq:Host"] ?? throw new Exception("HostName is not configured");
        var port = int.Parse(configuration["RabbitMq:Port"] ?? throw new Exception("Port is not configured"));
        var userName = configuration["RabbitMq:Username"] ?? throw new Exception("Username is not configured");
        var password = configuration["RabbitMq:Password"] ?? throw new Exception("Password is not configured");
        var queueName = configuration["RabbitMq:QueueName"] ?? throw new Exception("QueueName is not configured");

        // RabbitMQ UI管理界面地址：https://rabbitmq.limit-dancer.com/ 用户名：root 密码：Limit@2016
        services.AddRabbitMq<TestRequest, TestRequestHanlder>(options =>
        {
            options.UseSsl = false;
            options.HostName = hostName;
            options.Port = port;
            options.UserName = userName;
            options.Password = password;
            options.Durable = true;
            options.NetworkRecoveryInterval = 10000;
            options.ExchangeType = ExchangeType.Direct;
            options.QueueName = queueName;
            options.Exchange = $"{queueName}_SERVICE_EXCHANGE";
            options.RoutingKey = $"{queueName}_ROUTING_KEY";
            options.DeadLetterExchange = $"{queueName}_SERVICE_EXCHANGE_DEAD";
            options.DeadLetterQueueName = $"{queueName}_DEAD";
            options.DeadLetterRoutingKey = $"{queueName}_ROUTING_KEY";
        });

        return services;
    }
}