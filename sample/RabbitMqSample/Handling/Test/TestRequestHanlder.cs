namespace RabbitMqSample.Handling.Test;

public class TestRequestHanlder : IRequestProcessorHandler<TestRequest>
{
    private readonly ILogger<TestRequestHanlder> _logger;

    public TestRequestHanlder(ILogger<TestRequestHanlder> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(int milliseconds, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task HandleAsync(TestRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"开始处理消息: {request.Id}");

        //模拟处理消息耗时操作
        await Task.Delay(1000, cancellationToken);

        _logger.LogInformation($"消息处理完成: {request.Id}");
    }
}