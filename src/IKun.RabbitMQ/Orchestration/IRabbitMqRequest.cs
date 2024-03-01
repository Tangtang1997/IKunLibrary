namespace IKun.RabbitMQ.Orchestration
{
    /// <summary>
    /// RabbitMQ请求
    /// </summary>
    public interface IRabbitMqRequest
    {
        /// <summary>
        /// 重试次数
        /// </summary>
        int RetryCount { get; set; }
    }
}