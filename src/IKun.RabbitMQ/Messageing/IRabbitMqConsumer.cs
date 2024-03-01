namespace IKun.RabbitMQ.Messageing
{
    /// <summary>
    /// RabbitMQ Consumer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRabbitMqConsumer<T> where T : IRabbitMqRequest
    {
        /// <summary>
        /// RabbitMQ Configuration
        /// </summary>
        IRabbitMqOptions RabbitMqOptions { get; }

        /// <summary>
        /// Request Received Event
        /// </summary>
        event Func<byte[], Task> RequestReceivedAsync;

        /// <summary>
        /// Start
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stop
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StopAsync(int milliseconds, CancellationToken cancellationToken = default);
    }
}