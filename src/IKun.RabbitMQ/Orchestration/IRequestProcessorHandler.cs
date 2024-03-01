namespace IKun.RabbitMQ.Orchestration
{
    /// <summary>
    /// 请求处理器处理程序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRequestProcessorHandler<in T>
    {
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
        /// 处理
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task HandleAsync(T request, CancellationToken cancellationToken = default);
    }
}