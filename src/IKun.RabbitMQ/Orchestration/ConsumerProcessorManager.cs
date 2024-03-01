namespace IKun.RabbitMQ.Orchestration
{
    /// <summary>
    /// 消费者处理器管理器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConsumerProcessorManager<T> : IConsumerProcessorManager<T> where T : IRabbitMqRequest
    {
        private readonly ILogger<ConsumerProcessorManager<T>> _logger;
        private readonly int _maxRequestRetryCount;

        /// <summary>
        /// 请求消费者
        /// </summary>
        public IRequestConsumer<T> RequestConsumer { get; }

        /// <summary>
        /// 请求处理器
        /// </summary>
        public IRequestProcessorHandler<T> ProcessorHandler { get; }

        /// <summary>
        /// 消费者处理器管理器
        /// </summary>
        /// <param name="requestConsumer"></param>
        /// <param name="processorHandler"></param>
        /// <param name="logger"></param>
        /// <param name="MaxRequestRetryCount"></param>
        public ConsumerProcessorManager(
            IRequestConsumer<T> requestConsumer,
            IRequestProcessorHandler<T> processorHandler,
            ILogger<ConsumerProcessorManager<T>> logger,
            int MaxRequestRetryCount = 0)
        {
            _logger = logger;
            _maxRequestRetryCount = MaxRequestRetryCount;
            RequestConsumer = requestConsumer;
            ProcessorHandler = processorHandler;
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await InitializeServiceAsync(cancellationToken);
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
        /// <param name="milliseconds"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(int milliseconds, CancellationToken cancellationToken)
        {
            _logger.LogInformation($" -> ConsumerProcessorManager -> Stop - Method start");

            try
            {
                RequestConsumer.RequestReceivedAsync -= OnRequestReceivedAsync;
                await RequestConsumer.StopAsync(milliseconds, cancellationToken);
                await ProcessorHandler.StopAsync(milliseconds, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $" -> ConsumerProcessorManager -> Stop - An error occured when stopping");
            }
            finally
            {
                _logger.LogInformation($" -> ConsumerProcessorManager -> Stop - Method completed");
            }
        }

        /// <summary>
        /// 初始化服务
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task InitializeServiceAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Run(() =>
                {
                    ProcessorHandler.StartAsync(cancellationToken);
                    RequestConsumer.RequestReceivedAsync += OnRequestReceivedAsync;
                    RequestConsumer.StartAsync(cancellationToken);
                }, cancellationToken);
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
        /// 请求接收
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual async Task OnRequestReceivedAsync(T request)
        {
            var jsonMessage = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            });

            try
            {
                request.RetryCount++;

                _logger.LogInformation($"Request received: {jsonMessage}");

                await ProcessorHandler.HandleAsync(request);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Bad Request") || e.Message.Contains("Unauthorized"))
                {
                    request.RetryCount = _maxRequestRetryCount;
                }

                if (request.RetryCount >= _maxRequestRetryCount)
                {
                    _logger.LogError(e, $" -> ConsumerProcessorManager -> OnRequestReceived - request was processed. {jsonMessage}");
                }
                else
                {
                    _logger.LogError(e, $" -> ConsumerProcessorManager -> OnRequestReceived - request was processed. {jsonMessage}");
                    await RequestConsumer.RetryAsync(request);
                }
            }
        }
    }
}