namespace IKun.RabbitMQ.Messageing
{
    /// <summary>
    /// RabbitMQ Client 配置
    /// </summary>
    public interface IRabbitMqOptions
    {
        /// <summary>
        /// 连接到的主机
        /// </summary>
        string HostName { get; set; }

        /// <summary>
        /// 要连接的端口
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// 在此连接期间要访问的虚拟主机
        /// </summary>
        string VirtualHost { get; set; }

        /// <summary>
        /// 向服务器进行身份验证时使用的用户名
        /// </summary>
        string UserName { get; set; }

        /// <summary>
        /// 对服务器进行身份验证时使用的密码
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// 证书指纹
        /// </summary>
        string? CertficateThumbPrint { get; set; }

        /// <summary>
        /// 证书名称
        /// </summary>
        string? CertSubject { get; set; }

        /// <summary>
        /// 控制是否确实应该使用TLS，设置为false将禁用连接上的TLS
        /// </summary>
        bool UseSsl { get; set; }

        /// <summary>
        /// 设置为false表示禁用自动连接恢复，默认为true
        /// </summary>
        bool AutomaticRecoveryEnabled { get; set; }

        /// <summary>
        /// 设置为false将使自动连接恢复不恢复拓扑(交换，队列，绑定等)。默认为true。
        /// </summary>
        bool TopologyRecoveryEnabled { get; set; }

        /// <summary>
        /// 客户端在重新尝试恢复连接之前等待的时间。
        /// </summary>
        ushort NetworkRecoveryInterval { get; set; }

        /// <summary>
        /// 与服务器协商时使用的心跳超时。
        /// </summary>
        ushort RequestedHeartbeat { get; set; }

        /// <summary>
        /// 交换机类型
        /// </summary>
        string ExchangeType { get; set; }

        /// <summary>
        /// 交换机名称
        /// </summary>
        string Exchange { get; set; }

        /// <summary>
        /// 路由键
        /// </summary>
        string RoutingKey { get; set; }

        /// <summary>
        /// 队列名称
        /// </summary>
        string QueueName { get; set; }

        /// <summary>
        /// 消费者预取计数
        /// </summary>
        ushort PrefetchCount { get; set; }

        /// <summary>
        /// 是否开启消息持久化
        /// </summary>
        bool Durable { get; set; }

        /// <summary>
        /// 死信交换机名称
        /// </summary>
        string DeadLetterExchange { get; set; }

        /// <summary>
        /// 死信交换机类型
        /// </summary>
        string DeadLetterRoutingKey { get; set; }

        /// <summary>
        /// 死信队列名称
        /// </summary>
        string DeadLetterQueueName { get; set; }

        /// <summary>
        /// 队列过期时间
        /// </summary>
        int QueueTtl { get; set; }

        /// <summary>
        /// 发布者获取超时时间
        /// </summary>
        int PublisherAcquireTimeoutMilliseconds { get; set; }

        /// <summary>
        /// 消费者数量
        /// </summary>
        int ConsumerCount { get; set; }

        /// <summary>
        /// 重新创建连接延迟毫秒数
        /// </summary>
        int RetryCreateDelayMilliseconds { get; set; }

        /// <summary>
        /// 允许最大请求重试次数
        /// </summary>
        int MaxRequestRetryCount { get; set; }
    }
}