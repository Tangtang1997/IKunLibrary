namespace IKun.Kafka.Messageing;

/// <summary>
/// kafka生产者
/// </summary>
public interface IKafkaProducer<T> where T : IKafkaRequest
{
    /// <summary>
    /// kafka配置
    /// </summary>
    KafkaOptions KafkaOptions { get; }

    /// <summary>
    /// 发布消息
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishAsync(string topic, object message, CancellationToken cancellationToken = default);

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