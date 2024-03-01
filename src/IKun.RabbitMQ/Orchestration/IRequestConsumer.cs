namespace IKun.RabbitMQ.Orchestration
{
    /// <summary>
    /// 请求消费者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRequestConsumer<T> where T : IRabbitMqRequest
    {
        /// <summary>
        /// 请求接收事件
        /// </summary>
        event Func<T, Task> RequestReceivedAsync;

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StopAsync(int milliseconds, CancellationToken cancellationToken = default);

        /// <summary>
        /// 重试
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RetryAsync(T request, CancellationToken cancellationToken = default);
    }
}