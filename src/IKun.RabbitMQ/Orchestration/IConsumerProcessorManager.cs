namespace IKun.RabbitMQ.Orchestration
{
    /// <summary>
    /// 消费者处理器管理器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConsumerProcessorManager<T> where T : IRabbitMqRequest
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
        /// <param name="milliseconds"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StopAsync(int milliseconds, CancellationToken cancellationToken);
    }
}