namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHostedService<SampleHostedService>();

        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

        // Kafka UI管理界面地址：http://124.223.225.27:8086/
        services.AddKafka<TestRequest, TestRequestHanlder>(options =>
        {
            options.BootstrapServers = configuration["Kafka:BootstrapServers"] ?? throw new Exception("Kafka BootstrapServers is not configured");
            options.Topic = configuration["Kafka:Topic"] ?? throw new Exception("Kafka Topic is not configured");
            options.GroupId = configuration["Kafka:GroupId"] ?? throw new Exception("Kafka GroupId is not configured");
            options.AllowAutoCreateTopics = true;
            options.EnableAutoCommit = false;
            options.EnablePartitionEof = true;
            options.MaxPollIntervalMs = 3600000;
            options.SessionTimeoutMs = 45000;
            options.HeartbeatIntervalMs = 15000;
            options.StatisticsIntervalMs = 3000;
            options.NetworkRecoveryInterval = 10000;
        });

        return services;
    }
}