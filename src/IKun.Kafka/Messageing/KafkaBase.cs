namespace IKun.Kafka.Messageing;

/// <summary>
/// kafka基类
/// </summary>
public abstract class KafkaBase : IDisposable
{
    private readonly ILogger<KafkaBase> _logger;

    /// <summary>
    /// kafka配置
    /// </summary>
    public KafkaOptions KafkaOptions { get; }

    /// <summary>
    /// 服务是否正在停止
    /// </summary>
    protected bool ServiceStopping = false;

    /// <summary>
    /// 重连事件
    /// </summary>
    protected ManualResetEvent ManualResetEvent = new(false);

    /// <summary>
    /// 连接检查线程是否创建
    /// </summary>
    private bool _connectionCheckThreadCreated;

    /// <summary> 
    /// ctor
    /// </summary>
    /// <param name="kafkaOptions"></param>
    /// <param name="logger"></param>
    protected KafkaBase(
        KafkaOptions kafkaOptions,
        ILogger<KafkaBase> logger)
    {
        _logger = logger;
        KafkaOptions = kafkaOptions;
    }

    /// <summary>
    /// 停止
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 重连接
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task ReconnectAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (ServiceStopping)
                {
                    return;
                }

                await InitConnectAsync(cancellationToken);
                SetConnectionCheck(cancellationToken);

                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{KafkaOptions.BootstrapServers}-{KafkaOptions.Topic}: Kafka reconnection failed!");
                await Task.Delay(KafkaOptions.RetryCreateDelayMilliseconds, cancellationToken);
            }
        }
    }

    /// <summary>
    /// 创建连接检查线程
    /// </summary>
    /// <param name="cancellationToken"></param>
    protected void SetConnectionCheck(CancellationToken cancellationToken = default)
    {
        if (_connectionCheckThreadCreated)
        {
            return;
        }

        Task.Factory.StartNew(
            function: async () =>
            {
                await ConnectionCheckAsync(cancellationToken);
            },
            cancellationToken: cancellationToken
            );

        _connectionCheckThreadCreated = true;
    }

    /// <summary>
    /// 初始化连接
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task InitConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 连接检查
    /// </summary>
    /// <param name="cancellationToken"></param>
    protected abstract Task ConnectionCheckAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查空值
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual void CheckNullValue()
    {
        if (string.IsNullOrEmpty(KafkaOptions.BootstrapServers))
        {
            throw new ArgumentNullException(nameof(KafkaOptions.BootstrapServers), "Kafka options: BootstrapServers is null");
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}