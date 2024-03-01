namespace IKun.RabbitMQ.Messageing
{
    /// <summary>
    /// RabbitMQ连接代理
    /// </summary>
    public interface IRabbitMqConnectionProxy
    {
        /// <summary>
        /// RabbitMQ配置
        /// </summary>
        IRabbitMqOptions RabbitMqOptions { get; }

        /// <summary>
        /// 创建连接
        /// </summary>
        /// <returns></returns>
        IConnection CreateConnection();
    }
}