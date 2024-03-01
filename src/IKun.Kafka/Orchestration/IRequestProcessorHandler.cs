namespace IKun.Kafka.Orchestration;

/// <summary>
/// 消息处理器
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IRequestProcessorHandler<in T> where T : IKafkaRequest
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

    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task HandleAsync(T request);
}