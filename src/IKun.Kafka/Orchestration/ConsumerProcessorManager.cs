namespace IKun.Kafka.Orchestration;

public class ConsumerProcessorManager<T> : IConsumerProcessorManager<T> where T : IKafkaRequest
{
    protected IRequestConsumer<T> Consumer;
    protected IRequestProcessorHandler<T> ProcessorHandler;

    private readonly ILogger<ConsumerProcessorManager<T>> _logger;
    private readonly string _name;
    private readonly int _maxRequestRetryCount;

    public ConsumerProcessorManager(
        IRequestConsumer<T> consumer,
        IRequestProcessorHandler<T> processorHandler,
        ILogger<ConsumerProcessorManager<T>> logger,
        string name = nameof(ConsumerProcessorManager<T>),
        int maxRequestRetryCount = 0)
    {
        ProcessorHandler = processorHandler;
        Consumer = consumer;

        _logger = logger;
        _name = name;
        _maxRequestRetryCount = maxRequestRetryCount;
    }

    /// <summary>
    /// 启动
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Starting ConsumerProcessorManager -> {_name}");

        try
        {
            await Task.Factory.StartNew(() =>
            {
                ProcessorHandler.StartAsync(cancellationToken);
                Consumer.RequestReceivedAsync += OnRequestReceivedAsync;
                Consumer.StartAsync(cancellationToken);
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException e)
        {
            _logger.LogError(e, $"Canceled ConsumerProcessorManager: {e.Message}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error starting ConsumerProcessorManager: {e.Message}");
        }
    }

    /// <summary>
    /// 停止
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Stopping ConsumerProcessorManager");

        try
        {
            Consumer.RequestReceivedAsync -= OnRequestReceivedAsync;
            await Consumer.StopAsync(cancellationToken);
            await ProcessorHandler.StopAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error stopping ConsumerProcessorManager: {e.Message}");
        }

        _logger.LogWarning("ConsumerProcessorManager stopped");
    }

    /// <summary>
    /// 消息接收
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task OnRequestReceivedAsync(T request)
    {
        try
        {
            _logger.LogInformation("Processing request.");

            await ProcessorHandler.HandleAsync(request);

            _logger.LogInformation("Processing request completed.");
        }
        catch (Exception e)
        {
            if (request.RetryCount >= _maxRequestRetryCount)
            {
                _logger.LogError(e, $"Error processing request: {e.Message}.");
                return;
            }

            request.RetryCount++;
            await Consumer.RetryAsync(request);

            _logger.LogWarning(e, $"Error processing request: {e.Message}. Retry count: {request.RetryCount}.");
        }
    }
}