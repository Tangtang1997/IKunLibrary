namespace IKun.RabbitMQ.Messageing
{
    /// <summary>
    /// RabbitMQ Producer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRabbitMqProducer<T> where T : IRabbitMqRequest
    {
        /// <summary>
        /// RabbitMQ Configuration
        /// </summary>
        IRabbitMqOptions RabbitMqOptions { get; }

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

        /// <summary>
        /// Publish
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey"></param>
        /// <param name="properties"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task PublishAsync(byte[] message, string exchangeName, string routingKey, IBasicProperties? properties = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Publish
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey"></param>
        /// <param name="properties"></param>
        void Publish(byte[] message, string exchangeName, string routingKey, IBasicProperties? properties = null);
    }
}