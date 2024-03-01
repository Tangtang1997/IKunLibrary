namespace IKun.RabbitMQ.Dispatch
{
    /// <summary>
    /// 请求消费者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RequestCosumer<T> : IRequestConsumer<T> where T : IRabbitMqRequest
    {
        private readonly IRabbitMqProducer<T> _producer;
        private readonly IRabbitMqConsumer<T> _consumer;
        private readonly int _stopTimeoutMilliseconds;

        /// <summary>
        /// 请求接收事件
        /// </summary>
        public event Func<T, Task> RequestReceivedAsync = null!;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="producer"></param>
        /// <param name="consumer"></param>
        /// <param name="stopTimeoutMilliseconds"></param>
        public RequestCosumer(
            IRabbitMqProducer<T> producer,
            IRabbitMqConsumer<T> consumer,
            int stopTimeoutMilliseconds = 30000)
        {
            _producer = producer;
            _consumer = consumer;
            _stopTimeoutMilliseconds = stopTimeoutMilliseconds;
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await _producer.StartAsync(cancellationToken);
            _consumer.RequestReceivedAsync += OnRequestReceivedAsync;
            await _consumer.StartAsync(cancellationToken);
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(int milliseconds, CancellationToken cancellationToken = default)
        {
            await _producer.StopAsync(_stopTimeoutMilliseconds, cancellationToken);
            _consumer.RequestReceivedAsync -= OnRequestReceivedAsync;
            await _consumer.StopAsync(_stopTimeoutMilliseconds, cancellationToken);
        }

        /// <summary>
        /// 重试
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task RetryAsync(T request, CancellationToken cancellationToken = default)
        {
            var body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(
                    request,
                    new JsonSerializerOptions
                    {
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    })
                );

            await _producer.PublishAsync(
                body,
                _consumer.RabbitMqOptions.Exchange,
                _consumer.RabbitMqOptions.RoutingKey,
                cancellationToken: cancellationToken
                );
        }

        /// <summary>
        /// 请求接收
        /// </summary>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        private async Task OnRequestReceivedAsync(byte[] requestBody)
        {
            var request = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(requestBody));
            await RequestReceivedAsync.Invoke(request!);
        }
    }
}