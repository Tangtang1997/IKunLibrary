namespace IKun.RabbitMQ.Messageing
{
    /// <summary>
    /// RabbitMQ Client 配置
    /// </summary>
    public class RabbitMqOptions : IRabbitMqOptions
    {
        /// <summary>
        /// 连接到的主机
        /// </summary>
        public string HostName { get; set; } = null!;

        /// <summary>
        /// 要连接的端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 在此连接期间要访问的虚拟主机
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// 向服务器进行身份验证时使用的用户名
        /// </summary>
        public string UserName { get; set; } = null!;

        /// <summary>
        /// 对服务器进行身份验证时使用的密码
        /// </summary>
        public string Password { get; set; } = null!;

        /// <summary>
        /// 证书指纹
        /// </summary>
        public string? CertficateThumbPrint { get; set; }

        /// <summary>
        /// 证书名称
        /// </summary>
        public string? CertSubject { get; set; }

        /// <summary>
        /// 控制是否确实应该使用TLS，设置为false将禁用连接上的TLS
        /// </summary>
        public bool UseSsl { get; set; } = false;

        /// <summary>
        /// 设置为false表示禁用自动连接恢复，默认为true
        /// </summary>
        public bool AutomaticRecoveryEnabled { get; set; }

        /// <summary>
        /// 设置为false将使自动连接恢复不恢复拓扑(交换，队列，绑定等)。默认为true。
        /// </summary>
        public bool TopologyRecoveryEnabled { get; set; }

        /// <summary>
        /// 客户端在重新尝试恢复连接之前等待的时间。
        /// </summary>
        public ushort NetworkRecoveryInterval { get; set; } = 10;

        /// <summary>
        /// 与服务器协商时使用的心跳超时。
        /// </summary>
        public ushort RequestedHeartbeat { get; set; }

        /// <summary>
        /// 交换机类型
        /// </summary>
        public string ExchangeType { get; set; } = null!;

        /// <summary>
        /// 交换机名称
        /// </summary>
        public string Exchange { get; set; } = null!;

        /// <summary>
        /// 路由键
        /// </summary>
        public string RoutingKey { get; set; } = null!;

        /// <summary>
        /// 队列名称
        /// </summary>
        public string QueueName { get; set; } = null!;

        /// <summary>
        /// 消费者预取计数
        /// </summary>
        public ushort PrefetchCount { get; set; }

        /// <summary>
        /// 是否开启消息持久化
        /// </summary>
        public bool Durable { get; set; }

        /// <summary>
        /// 死信交换机名称
        /// </summary>
        public string DeadLetterExchange { get; set; } = null!;

        /// <summary>
        /// 死信交换机类型
        /// </summary>
        public string DeadLetterRoutingKey { get; set; } = null!;

        /// <summary>
        /// 死信队列名称
        /// </summary>
        public string DeadLetterQueueName { get; set; } = null!;

        /// <summary>
        /// 队列过期时间
        /// </summary>
        public int QueueTtl { get; set; } = 3600000;

        /// <summary>
        /// 发布者获取超时时间
        /// </summary>
        public int PublisherAcquireTimeoutMilliseconds { get; set; }

        /// <summary>
        /// 消费者数量
        /// </summary>
        public int ConsumerCount { get; set; }

        /// <summary>
        /// 重新创建连接延迟毫秒数
        /// </summary>
        public int RetryCreateDelayMilliseconds { get; set; } = 10000;

        /// <summary>
        /// 允许最大请求重试次数
        /// </summary>
        public int MaxRequestRetryCount { get; set; }
    }
}