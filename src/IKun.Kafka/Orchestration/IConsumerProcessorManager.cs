namespace IKun.Kafka.Orchestration;

/// <summary>
/// kafka消费者处理器管理
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IConsumerProcessorManager<T> where T : IKafkaRequest
{
    /// <summary>
    /// 启动
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 停止
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StopAsync(CancellationToken cancellationToken);
} 