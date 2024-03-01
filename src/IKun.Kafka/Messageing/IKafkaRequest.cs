namespace IKun.Kafka.Messageing;

/// <summary>
/// kafka请求
/// </summary>
public interface IKafkaRequest
{
    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; }
}