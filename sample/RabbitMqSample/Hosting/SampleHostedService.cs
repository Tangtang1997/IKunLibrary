namespace RabbitMqSample.Hosting;

public class SampleHostedService : IHostedService
{
    private readonly IConsumerProcessorManager<TestRequest> _consumerProcessorManager;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<SampleHostedService> _logger;

    public SampleHostedService(
        IConsumerProcessorManager<TestRequest> consumerProcessorManager,
        IHostApplicationLifetime applicationLifetime,
        ILogger<SampleHostedService> logger)
    {
        _consumerProcessorManager = consumerProcessorManager;
        _applicationLifetime = applicationLifetime;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _applicationLifetime.ApplicationStarted.Register(() =>
        {
            _logger.LogInformation("SampleHostedService is starting.");
            _consumerProcessorManager.StartAsync(cancellationToken);
        });

        _applicationLifetime.ApplicationStopping.Register(() =>
        {
            _logger.LogInformation("SampleHostedService is stopping.");
            _consumerProcessorManager.StopAsync(3000, cancellationToken);
        });

        await Task.CompletedTask;   
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}