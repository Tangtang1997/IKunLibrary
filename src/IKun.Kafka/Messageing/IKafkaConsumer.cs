namespace IKun.Kafka.Messageing;

/// <summary>
/// kafka消费者
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IKafkaConsumer<out T> where T : IKafkaRequest
{
    /// <summary>
    /// kafka配置
    /// </summary>
    KafkaOptions KafkaOptions { get; }

    /// <summary>
    /// 消息接收事件
    /// </summary>
    event Func<T, Task> RequestReceivedAsync;
     
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