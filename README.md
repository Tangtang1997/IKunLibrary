# IKunLibrary

![](https://minio-api.limit-dancer.com/images/rabbitmq-logo.png?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=BZEIJS5MVW0GDKO89QRG%2F20240311%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20240311T064332Z&X-Amz-Expires=604800&X-Amz-Security-Token=eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJhY2Nlc3NLZXkiOiJCWkVJSlM1TVZXMEdES084OVFSRyIsImV4cCI6MTcxMDE0MjUxNCwicGFyZW50IjoiYWRtaW4ifQ.b9mTJHPIq7rodXiWuZiTJUqwlmjxHN00AWa3li2Zin0tzEJW2zsv29hY8NaFkL84OuiQDT8i3mawv1e0nDCu3w&X-Amz-SignedHeaders=host&versionId=null&X-Amz-Signature=3238f61440f9523d77bbbe2b596669f1222ed60fe94dea7860e6f5515f8532ac)
@import "[TOC]" {cmd="toc" depthFrom=1 depthTo=6 orderedList=false}
[TOC]

## 1.引言

[RabbitMQ](https://www.rabbitmq.com/) 是一个可靠且成熟的消息传递和流代理，它很容易部署在云环境、内部部署和本地机器上。它目前被全世界数百万人使用。

-   **可靠性**
    <p>RabbitMQ提供了多种技术可以让你在性能和可靠性之间进行权衡。这些技术包括持久性机制、投递确认、发布者证实和高可用性机制。</p>
-   **灵活的路由**
    <p>消息在到达队列前是通过交换机进行路由的。RabbitMQ为典型的路由逻辑提供了多种内置交换机类型。如果你有更复杂的路由需求，可以将这些交换机组合起来使用，你甚至可以实现自己的交换机类型，并且当做RabbitMQ的插件来使用。</p>
-   **集群**
    <p>在相同局域网中的多个RabbitMQ服务器可以聚合在一起，作为一个独立的逻辑代理来使用。</p>
-   **联合**
    <p>对于服务器来说，它比集群需要更多的松散和非可靠链接。为此 RabbitMQ 提供了联合模型。</p>
-   **高可用的队列**
    <p>在同一个集群里，队列可以被镜像到多个机器中，以确保当其中某些硬件出现故障后，你的消息仍然安全。</p>
-   **多协议**
     <p>RabbitMQ支持多种消息传递协议。AMQP是RabbitMQ的核心协议，但是RabbitMQ也支持STOMP、MQTT、HTTP等多种协议。</p>
-   **多语言客户端**
    <p>RabbitMQ提供了多种语言的客户端，包括Java、.NET、Python、PHP、Ruby、JavaScript、Erlang、Swift、Go等。</p>
-   **管理界面**
    <p>RabbitMQ提供了一个易用的管理界面，可以让你监控和管理RabbitMQ服务器。</p>
-   **插件**
    <p>RabbitMQ提供了多种插件，可以让你扩展RabbitMQ的功能。这些插件包括支持新的协议、新的后端存储、新的交换机类型、新的工具等。</p>
-   **社区**
    <p>RabbitMQ有一个活跃的社区，你可以在这里找到很多有用的信息。</p>
-   **可扩展性**
     <p>RabbitMQ可以很容易地扩展，你可以在需要的时候增加或减少服务器。</p>

## 2.基本概念

-   **生产者（Producer）**
    <p>生产者是一个发送消息的程序。发送消息的程序可以是任何语言编写的，只要它能够连接到RabbitMQ服务器，并且能够发送消息到RabbitMQ服务器。</p>
-   **消费者（Consumer）**
    <p>消费者是一个接收消息的程序。接收消息的程序可以是任何语言编写的，只要它能够连接到RabbitMQ服务器，并且能够从RabbitMQ服务器接收消息。</p>
-   **队列（Queue）**
    <p>队列是RabbitMQ的内部对象，用于存储消息。多个生产者可以向一个队列发送消息，多个消费者可以尝试从一个队列接收消息。队列支持多种消息分发策略。</p>
-   **交换机（Exchange）**
    <p>交换机是消息的分发中心。它接收来自生产者的消息，然后将这些消息分发给队列。交换机有多种类型，包括直连交换机、主题交换机、扇形交换机、头交换机。</p>
-   **绑定（Binding）**
    <p>绑定是交换机和队列之间的关联关系。绑定可以使用路由键进行绑定，也可以使用通配符进行绑定。</p>
-   **路由键（Routing Key）**
    <p>路由键是生产者发送消息时附带的一个属性。路由键的作用是决定消息被分发到哪个队列。</p>
-   **通配符（Wildcard）**
    <p>通配符是一种模式匹配的方式。RabbitMQ支持两种通配符：`*`和`#`。</p>
-   **绑定键（Binding Key）**
    <p>绑定键是交换机和队列之间的关联关系。绑定键可以使用路由键进行绑定，也可以使用通配符进行绑定。</p>
-   **持久化（Durable）**
    <p>持久化是指RabbitMQ服务器重启后，消息是否还存在。持久化可以应用到交换机、队列、绑定、消息等。</p>
-   **确认机制（Acknowledge）**
    <p>确认机制是指消费者接收到消息后，向RabbitMQ服务器发送一个确认消息。RabbitMQ服务器收到确认消息后，会删除这条消息。</p>

    -   **自动确认**
        <p>消费者接收到消息后，RabbitMQ服务器会自动删除这条消息。</p>
    -   **手动确认**
        <p>消费者接收到消息后，需要向RabbitMQ服务器发送一个确认消息。RabbitMQ服务器收到确认消息后，会删除这条消息。</p>

-   **拒绝机制（Reject）**
    <p>拒绝机制是指消费者接收到消息后，向RabbitMQ服务器发送一个拒绝消息。RabbitMQ服务器收到拒绝消息后，会将这条消息重新发送给其他消费者。</p>
-   **死信队列（Dead Letter Queue）**
    <p>死信队列是指消息被拒绝、过期或者达到最大重试次数后，会被发送到死信队列。</p>
-   **消息过期（Message TTL）**
    <p>消息过期是指消息在指定时间内没有被消费者消费，会被删除。</p>
-   **消息优先级（Message Priority）**
    <p>消息优先级是指消息在队列中的优先级。消息优先级高的消息会被优先消费。</p>
-   **消息分发**
      <p>消息分发是指消息在队列中的分发策略。消息分发策略包括轮询分发、公平分发、负载均衡分发。</p>

## 3.环境搭建

-   **Docker 安装 RabbitMQ**

    ```shell
    docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 --restart=always --hostname my-rabbit -e RABBITMQ_DEFAULT_USER=admin -e RABBITMQ_DEFAULT_PASS=admin -e TZ=Asia/Shanghai rabbitmq:management
    ```

    -   `-d`：后台运行
    -   `--restart`：重启策略
    -   `--name`：容器名称
    -   `-p`：端口映射
    -   `--hostname`：主机名
    -   `-e`：环境变量
        -   `RABBITMQ_DEFAULT_USER`：默认用户名
        -   `RABBITMQ_DEFAULT_PASS`：默认密码
        -   `TZ`：时区
    -   `rabbitmq:management`：镜像名称

-   **Docker Compose 安装 RabbitMQ**

    ```yaml
    version: "3.1"
    services:
        rabbitmq:
            restart: always
            image: rabbitmq:management
            container_name: rabbitmq
            hostname: my-rabbit
            ports:
                - 5672:5672
                - 15672:15672 # RabbitMQ管理界面端口
            environment:
                TZ: Asia/Shanghai
                RABBITMQ_DEFAULT_USER: admin
                RABBITMQ_DEFAULT_PASS: admin
    ```

    -   `restart`：重启策略
    -   `image`：镜像名称
    -   `container_name`：容器名称
    -   `hostname`：主机名
    -   `ports`：端口映射
    -   `environment`：环境变量
        -   `TZ`：时区
        -   `RABBITMQ_DEFAULT_USER`：默认用户名
        -   `RABBITMQ_DEFAULT_PASS`：默认密码
    -   `rabbitmq:management`：镜像名称

## 4.使用

-   **新建 `TestRequest` 类，实现 `IRabbitMqRequest` 接口，定义消息体**

```csharp
public class TestRequest : IRabbitMqRequest
{
    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; }

    #region 自定义字段

    /// <summary>
    /// id
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// 年龄
    /// </summary>
    public int Age { get; set; }

    #endregion
}
```

-   **新建`TestRequestHandler`类，实现`IRabbitMqRequestHandler<TestRequest>`接口，处理消息**

```csharp
public class TestRequestHanlder : IRequestProcessorHandler<TestRequest>
{
    private readonly ILogger<TestRequestHanlder> _logger;

    public TestRequestHanlder(ILogger<TestRequestHanlder> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(int milliseconds, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task HandleAsync(TestRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"开始处理消息: {request.Id}");

        //模拟处理消息耗时操作
        await Task.Delay(1000, cancellationToken);

        _logger.LogInformation($"消息处理完成: {request.Id}");
    }
}
```

-   **使用 `IHostedService` 来托管服务**

```csharp
public class SampleHostedService : IHostedService
{
    private readonly IConsumerProcessorManager<TestRequest> _consumerProcessorManager;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<SampleHostedService> _logger;

    public SampleHostedService(
        IConsumerProcessorManager<TestRequest> consumerProcessorManager,
        IHostApplicationLifetime applicationLifetime,
        ILogger<SampleHostedService> logger)
    {
        _consumerProcessorManager = consumerProcessorManager;
        _applicationLifetime = applicationLifetime;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _applicationLifetime.ApplicationStarted.Register(() =>
        {
            _logger.LogInformation("SampleHostedService is starting.");
            _consumerProcessorManager.StartAsync(cancellationToken);
        });

        _applicationLifetime.ApplicationStopping.Register(() =>
        {
            _logger.LogInformation("SampleHostedService is stopping.");
            _consumerProcessorManager.StopAsync(3000, cancellationToken);
        });

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
```

-   **注册并启用服务**

```csharp
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<SampleHostedService>();

        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

        var hostName = configuration["RabbitMq:Host"] ?? throw new Exception("HostName is not configured");
        var port = int.Parse(configuration["RabbitMq:Port"] ?? throw new Exception("Port is not configured"));
        var userName = configuration["RabbitMq:Username"] ?? throw new Exception("Username is not configured");
        var password = configuration["RabbitMq:Password"] ?? throw new Exception("Password is not configured");
        var queueName = configuration["RabbitMq:QueueName"] ?? throw new Exception("QueueName is not configured");

        services.AddRabbitMq<TestRequest, TestRequestHanlder>(options =>
        {
            options.UseSsl = false;
            options.HostName = hostName;
            options.Port = port;
            options.UserName = userName;
            options.Password = password;
            options.Durable = true;
            options.NetworkRecoveryInterval = 10000;
            options.ExchangeType = ExchangeType.Direct;
            options.QueueName = queueName;
            options.Exchange = $"{queueName}_SERVICE_EXCHANGE";
            options.RoutingKey = $"{queueName}_ROUTING_KEY";
            options.DeadLetterExchange = $"{queueName}_SERVICE_EXCHANGE_DEAD";
            options.DeadLetterQueueName = $"{queueName}_DEAD";
            options.DeadLetterRoutingKey = $"{queueName}_ROUTING_KEY";
        });
    })
    .Build();

await host.RunAsync();
```
