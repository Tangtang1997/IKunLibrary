namespace IKun.Kafka.Messageing;

/// <summary>
/// kafka消费者
/// </summary>
/// <typeparam name="T"></typeparam>
public class KafkaConsumer<T> : KafkaBase, IKafkaConsumer<T> where T : IKafkaRequest
{
    /// <summary>
    /// 消息接收事件
    /// </summary>
    public event Func<T, Task> RequestReceivedAsync = null!;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger<KafkaConsumer<T>> _logger;

    /// <summary>
    /// 消费者
    /// </summary>
    private IConsumer<Ignore, T>? _consumer;

    /// <summary>
    /// 是否成功订阅
    /// </summary>
    private bool _isSubscribedSuccessfully;

    /// <summary>
    /// 未提交的消息队列
    /// </summary>
    private readonly BlockingCollection<ConsumeResult<Ignore, T>> _uncommitMessageQueue = new();

    public KafkaConsumer(
        KafkaOptions configuration,
        ILogger<KafkaConsumer<T>> logger)
        : base(
            configuration,
            logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 启动
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("KafkaConsumer started");

        await ReconnectAsync(cancellationToken);
    }

    /// <summary>
    /// 停止
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("KafkaConsumer stopping");

        ServiceStopping = true;

        try
        {
            ManualResetEvent.Set();

            _consumer?.Unsubscribe();
            _consumer?.Dispose();

            await base.StopAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error stopping KafkaConsumer: {e.Message}");
        }

        _logger.LogWarning("KafkaConsumer stopped");
    }

    /// <summary>
    /// 检查空值
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    protected sealed override void CheckNullValue()
    {
        base.CheckNullValue();

        if (string.IsNullOrEmpty(KafkaOptions.Topic))
        {
            throw new ArgumentNullException(nameof(KafkaOptions.Topic), "Kafka options: Topic is null");
        }
    }

    /// <summary>
    /// 初始化连接
    /// </summary>
    protected override async Task InitConnectAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"{KafkaOptions.BootstrapServers}-{KafkaOptions.Topic}: Kafka consumer create - Start");

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = KafkaOptions.BootstrapServers,
            GroupId = KafkaOptions.GroupId,
            AllowAutoCreateTopics = KafkaOptions.AllowAutoCreateTopics,
            EnableAutoCommit = KafkaOptions.EnableAutoCommit,
            EnablePartitionEof = KafkaOptions.EnablePartitionEof,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            MaxPollIntervalMs = KafkaOptions.MaxPollIntervalMs,
            SessionTimeoutMs = KafkaOptions.SessionTimeoutMs,
            StatisticsIntervalMs = KafkaOptions.StatisticsIntervalMs,
            HeartbeatIntervalMs = KafkaOptions.HeartbeatIntervalMs
        };

        var consumerBuilder = new ConsumerBuilder<Ignore, T>(consumerConfig)
            .SetErrorHandler((consumer, e) =>
            {
                _logger.LogError($"{KafkaOptions.BootstrapServers} - {consumer.Name} -> {JsonSerializer.Serialize(e)}");
            })
            .SetStatisticsHandler((consumer, _) =>
            {
#if DEBUG
                foreach (var topic in consumer.Subscription)
                {
                    Console.WriteLine($"{KafkaOptions.BootstrapServers} - {consumer.Name}-{topic} -> 消息监听中...... ");
                }
#endif
            })
            .SetPartitionsAssignedHandler((consumer, partitions) =>
            {
                _logger.LogInformation($"{KafkaOptions.BootstrapServers} - {consumer.Name} -> 分区: {string.Join(", ", partitions.Select(x => x.Partition.Value))}");
            })
            .SetValueDeserializer(new KafkaJsonDeserializer<T>(NullLogger<KafkaJsonDeserializer<T>>.Instance));

        var oldConsumer = _consumer;
        _consumer = consumerBuilder.Build();

        _logger.LogInformation($"{KafkaOptions.BootstrapServers}-{KafkaOptions.Topic}: Kafka consumer create - Finished");

        //如果旧消费者不为空，则先释放
        if (oldConsumer != null)
        {
            try
            {
                oldConsumer.Unsubscribe();
                oldConsumer.Dispose();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"KafkaConsumer - InitConnectAsync -> Disposing old consumer error: {e.Message}");
            }
        }

        await ConnectionReadyAsync(cancellationToken);
    }

    /// <summary>
    /// 连接检查
    /// </summary>
    /// <param name="cancellationToken"></param>
    protected override async Task ConnectionCheckAsync(CancellationToken cancellationToken = default)
    {
        Thread.CurrentThread.Name = $"{KafkaOptions.Topic}-Consumer-Check-Thread";
        
        _logger.LogInformation($"{KafkaOptions.BootstrapServers}-{KafkaOptions.Topic}: Kafka consumer connection check ThreadName:{Thread.CurrentThread.Name} - Start");

        while (!cancellationToken.IsCancellationRequested)
        {
            ManualResetEvent.WaitOne(KafkaOptions.NetworkRecoveryInterval);

            //如果服务正在停止，则退出
            if (ServiceStopping)
            {
                break;
            }

            //如果消费者不为空，并且成功订阅，则继续循环
            if (_consumer != null && _isSubscribedSuccessfully)
            {
                continue;
            }

            _logger.LogInformation($"{KafkaOptions.BootstrapServers}-{KafkaOptions.Topic}: Kafka consumer reconnect raised");

            //重连
            await ReconnectAsync(cancellationToken);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation($"Kafka consumer connection check ThreadName:{Thread.CurrentThread.Name} - Canceled");
        }
    }

    /// <summary>
    /// 连接就绪
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task ConnectionReadyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _consumer?.Subscribe(KafkaOptions.Topic);

            while (!cancellationToken.IsCancellationRequested)
            {
                var consumeResult = _consumer?.Consume(cancellationToken);

                //标记成功订阅
                _isSubscribedSuccessfully = true;

                switch (consumeResult)
                {
                    case null:
                        continue;
                    case
                    {
                        IsPartitionEOF: true
                    }:
#if DEBUG
                        _logger.LogInformation($"Reached end of topic {consumeResult.Topic}, partition {consumeResult.Partition}, offset {consumeResult.Offset}.");
#endif
                        continue;
                    default:
                        await OnReceiveCompletedAsync(consumeResult);
                        ProcessUnCommitMessage(cancellationToken);
                        break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Consumer cancelled");
            _consumer?.Dispose();
        }
        catch (ConsumeException e)
        {
            _logger.LogError(e, $"KafkaConsumer - ConnectionReadyAsync -> Error consuming message: {e.Error.Reason}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"KafkaConsumer - ConnectionReadyAsync -> Error consuming message: {ex.Message}");
        }
    }

    /// <summary>
    /// 接收完成，处理消息
    /// </summary>
    /// <param name="consumeResult"></param>
    /// <returns></returns>
    private async Task OnReceiveCompletedAsync(ConsumeResult<Ignore, T> consumeResult)
    {
        try
        {
            var data = consumeResult.Message.Value;

            if (data == null)
            {
                _logger.LogError($"Offset: {consumeResult.Offset}, 消息为空");
                _consumer?.Commit(consumeResult);
                return;
            }

            var dataJson = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            });

            _logger.LogInformation($"接收到消息  Offset: {consumeResult.Offset}, Value: {dataJson}");

            var isFired = await FireReceiveEventAsync(data);
            if (isFired)
            {
                try
                {
                    _consumer?.Commit(consumeResult);
                    _logger.LogInformation("Message commit successful. ");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Message commit failure: {e.Message}. Move it to queue and try later!");
                    _uncommitMessageQueue.Add(consumeResult);
                }
            }
            else
            {
                _logger.LogError("Message receive failure. ");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error consuming message: {e.Message}");
        }
    }

    /// <summary>
    /// 触发消息接收事件
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private async Task<bool> FireReceiveEventAsync(T data)
    {
        try
        {
            await RequestReceivedAsync.Invoke(data);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"KafkaConsumer - FireReceiveEventAsync -> Error consuming message: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 处理未提交的消息
    /// </summary>
    /// <param name="cancellationToken"></param>
    public void ProcessUnCommitMessage(CancellationToken cancellationToken = default)
    {
        var commitCount = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            if (ServiceStopping || _consumer == null)
            {
                break;
            }

            if (!_uncommitMessageQueue.TryTake(out var consumeResult))
            {
                break;
            }

            try
            {
                _consumer?.Commit(consumeResult);

                commitCount++;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"KafkaConsumer - ProcessUnCommitMessage -> Message commit faild: {e.Message}");
            }
        }

        if (commitCount > 0)
        {
            _logger.LogInformation($"KafkaConsumer - ProcessUnCommitMessage -> The {commitCount} message without commit was successfully processed!");
        }
    }
}