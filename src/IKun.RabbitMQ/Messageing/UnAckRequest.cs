namespace IKun.RabbitMQ.Messageing
{
    /// <summary>
    /// 未确认请求
    /// </summary>
    public class UnAckRequest
    {
        /// <summary>
        /// DeliveryTag
        /// </summary>
        public ulong DeliveryTag { get; set; }

        /// <summary>
        /// Timeout
        /// </summary>
        public DateTime Timeout { get; set; }

        /// <summary>
        /// 是否已经被处理
        /// </summary>
        public bool IsFired { get; set; }
    }
}