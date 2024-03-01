namespace IKun.Kafka.Messageing;

/// <summary>
/// kafka生产者
/// </summary>
public class KafkaProducer<T> : KafkaBase, IKafkaProducer<T> where T : IKafkaRequest
{
    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger<KafkaProducer<T>> _logger;

    /// <summary>
    /// 生产者
    /// </summary>
    private IProducer<string, string>? _producer;

    /// <summary>
    /// 未发布的消息
    /// </summary>
    private readonly BlockingCollection<UnpublishMessage> _unpublishedMessageQueue = new();

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="kafkaConfiguration"></param>
    /// <param name="logger"></param>
    public KafkaProducer(
        KafkaOptions kafkaConfiguration,
        ILogger<KafkaProducer<T>> logger)
        : base(
            kafkaConfiguration,
            logger)
    {
        CheckNullValue(); 

        _logger = logger;
    }

    /// <summary>
    /// 启动
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("KafkaProducer started");

        await ReconnectAsync(cancellationToken);
    }

    /// <summary>
    /// 停止
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("KafkaProducer stopping");

        ServiceStopping = true;

        try
        {
            ManualResetEvent.Set();

            _producer?.Flush(cancellationToken);
            _producer?.Dispose();

            await base.StopAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error stopping KafkaProducer: {e.Message}");
        }

        _logger.LogWarning("KafkaProducer stoped");
    }

    /// <summary>
    /// 发布消息
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task PublishAsync(string topic, object message, CancellationToken cancellationToken = default)
    {
        try
        {
            var messageJson = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            });

            _logger.LogInformation($"Publishing message to topic {topic}: {messageJson}");

            var kafkaMessage = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = messageJson
            };

            await _producer?.ProduceAsync(topic, kafkaMessage, cancellationToken)!;

            _producer?.Flush(cancellationToken);

            _logger.LogInformation($"Published message to topic {topic}");
        }
        catch (OperationCanceledException e)
        {
            _logger.LogError(e, $"Operation canceled publishing message to topic {topic}: {e.Message}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error publishing message to topic {topic}: {e.Message}. Move it to queue and try later!");

            _unpublishedMessageQueue.Add(new UnpublishMessage
            {
                Topic = topic,
                Message = message
            }, cancellationToken);
        }
    }

    /// <summary>
    /// 连接初始化
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task InitConnectAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"{KafkaOptions.BootstrapServers}-{KafkaOptions.Topic}: Kafka producer create - Start");

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = KafkaOptions.BootstrapServers,
            AllowAutoCreateTopics = KafkaOptions.AllowAutoCreateTopics,
            ClientId = KafkaOptions.GroupId
        };

        var oldProducer = _producer;
        _producer = new ProducerBuilder<string, string>(producerConfig).Build();

        _logger.LogInformation($"{KafkaOptions.BootstrapServers}-{KafkaOptions.Topic}: Kafka producer create - Finished");

        try
        {
            oldProducer?.Flush(cancellationToken);
            oldProducer?.Dispose();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"{KafkaOptions.BootstrapServers}-{KafkaOptions.Topic}: Error disposing old producer: {e.Message}");
        }

        await ProcessUnpublishMessageAsync(cancellationToken);
    }

    /// <summary>
    /// 连接检查
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task ConnectionCheckAsync(CancellationToken cancellationToken = default)
    {
        Thread.CurrentThread.Name = $"{KafkaOptions.Topic}-Producer-Check-Thread";
        _logger.LogInformation($"{KafkaOptions.BootstrapServers}-{KafkaOptions.Topic}: Kafka producer connection check ThreadName:{Thread.CurrentThread.Name} - Start");

        while (!cancellationToken.IsCancellationRequested)
        {
            ManualResetEvent.WaitOne(KafkaOptions.NetworkRecoveryInterval);

            //如果服务正在停止，则退出
            if (ServiceStopping)
            {
                break;
            }

            if (_producer == null)
            {
                _logger.LogInformation($"{KafkaOptions.BootstrapServers}-{KafkaOptions.Topic}: Kafka producer reconnect raised");

                await ReconnectAsync(cancellationToken);
            }
        }
    }

    /// <summary>
    /// 检查空值
    /// </summary>
    protected sealed override void CheckNullValue()
    {
        base.CheckNullValue();
    }

    /// <summary>
    /// 处理未发布的消息
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task ProcessUnpublishMessageAsync(CancellationToken cancellationToken = default)
    {
        var publishCount = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            if (ServiceStopping || _producer == null)
            {
                break;
            }

            if (!_unpublishedMessageQueue.TryTake(out var request))
            {
                break;
            }

            try
            {
                await PublishAsync(request.Topic, request.Message, cancellationToken);
                publishCount++;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error publishing message to topic {KafkaOptions.Topic}: {e.Message}");
            }
        }

        if (publishCount > 0)
        {
            _logger.LogInformation($"The {publishCount} message that did not publish was successfully processed!");
        }
    }

    public class UnpublishMessage
    {
        public string Topic { get; set; } = default!;

        public object Message { get; set; } = default!;
    }
}