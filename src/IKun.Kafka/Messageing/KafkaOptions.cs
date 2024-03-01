namespace IKun.Kafka.Messageing;

/// <summary>
/// kafka配置
/// </summary>
public class KafkaOptions
{
    /// <summary>
    /// kafka集群地址组： host1/port1,host2/port2,host3/port3
    /// </summary>
    public string BootstrapServers { get; set; } = string.Empty;

    /// <summary>
    /// 主题
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// 消费者组唯一标识
    /// </summary>
    public string GroupId { get; set; } = string.Empty;

    /// <summary>
    /// 是否自动提交offset
    /// </summary>
    public bool EnableAutoCommit { get; set; } = false;

    /// <summary>
    /// 如果两次poll操作间隔超过了这个时间，broker就会认为这个consumer处理能力太弱，会将其踢出消费组，将分区分配给别的consumer消费 ，触发rebalance 。
    /// </summary>
    public int MaxPollIntervalMs { get; set; } = 3600000;

    /// <summary>
    /// group coordinator检测consumer发生崩溃所需的时间。一个consumer group里面的某个consumer挂掉了，最长需要 session.timeout.ms 秒检测出来。
    /// 它指定了一个阈值，在这个阈值内如果group coordinator未收到consumer的任何消息（指心跳），那coordinator就认为consumer挂了。
    /// </summary>
    public int SessionTimeoutMs { get; set; } = 45000;

    /// <summary>
    /// 每个consumer 都会根据 heartbeat.interval.ms 参数指定的时间周期性地向group coordinator发送 hearbeat，group coordinator会给各个consumer响应，若发生了 rebalance，各个consumer收到的响应中会包含 REBALANCE_IN_PROGRESS 标识，这样各个consumer就知道已经发生了rebalance，同时 group coordinator也知道了各个consumer的存活情况。
    /// </summary>
    public int HeartbeatIntervalMs { get; set; } = 3000;

    /// <summary>
    /// Librdkafka统计数据发出间隔。
    /// 应用程序还需要使用ird_kafka_conf set_stats_cb0'注册一个stats回调。粒度为1000ms。值为0时禁用统计数据。default: 0重要性:高
    /// </summary>
    public int StatisticsIntervalMs { get; set; } = 3000;

    /// <summary>
    /// 当订阅或分配主题时，允许在代理上自动创建主题，默认为true。
    /// 只有在broker允许使用'auto.create.topics'时，订阅的主题才会自动创建。
    /// 当使用小于0.11.0的broker时，这个配置必须设置为“false”。
    /// </summary>
    public bool AllowAutoCreateTopics { get; set; }

    /// <summary>
    /// 每当消费者到达分区结束时，触发RD_KAFKA_RESP_ERR_PARTITION_EOF事件。默认值:false重要性:low
    /// </summary>
    public bool EnablePartitionEof { get; set; }

    /// <summary>
    /// 配置当消息处理失败时，重新投递该消息的次数
    /// </summary>
    public int MaxRequestRetryCount { get; set; }

    /// <summary>
    /// 重新创建连接延迟毫秒数
    /// </summary>
    public int RetryCreateDelayMilliseconds { get; set; } = 60000;

    /// <summary>
    /// 客户端在重新尝试恢复连接之前等待的时间。
    /// </summary>
    public int NetworkRecoveryInterval {get; set; } = 60000;
}