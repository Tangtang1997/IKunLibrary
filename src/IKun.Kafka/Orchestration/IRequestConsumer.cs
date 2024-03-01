namespace IKun.Kafka.Orchestration;

/// <summary>
/// 请求消费者（更高一层的消费者）
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IRequestConsumer<T> where T : IKafkaRequest
{
    /// <summary>
    /// 消息接收事件
    /// </summary>
    event Func<T,Task>  RequestReceivedAsync;

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

    /// <summary>
    /// 重试
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RetryAsync(T request, CancellationToken cancellationToken = default);
}