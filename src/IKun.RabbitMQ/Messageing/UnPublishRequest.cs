namespace IKun.RabbitMQ.Messageing
{
    /// <summary>
    /// 取消发布请求
    /// </summary>
    public class UnPublishRequest
    {
        /// <summary>
        /// 消息体
        /// </summary>
        public byte[]? RequestBody { get; set; }

        /// <summary>
        /// Timeout
        /// </summary>
        public DateTime Timeout { get; set; }

        /// <summary>
        /// 路由键
        /// </summary>
        public string? RoutingKey { get; set; }

        /// <summary>
        /// 交换机名称
        /// </summary>
        public string? ExchangeName { get; set; }
    }
}