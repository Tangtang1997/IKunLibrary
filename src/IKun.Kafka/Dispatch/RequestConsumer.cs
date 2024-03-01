namespace IKun.Kafka.Dispatch;

/// <summary>
/// 请求消费者
/// </summary>
/// <typeparam name="T"></typeparam>
public class RequestConsumer<T> : IRequestConsumer<T> where T : IKafkaRequest
{
    private readonly IKafkaProducer<T> _producer;
    private readonly IKafkaConsumer<T> _consumer;

    /// <summary>
    /// 消息接收事件
    /// </summary>
    public event Func<T, Task>? RequestReceivedAsync;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="producer"></param> 
    /// <param name="consumer"></param>
    public RequestConsumer(IKafkaProducer<T> producer, IKafkaConsumer<T> consumer)
    {
        _producer = producer;
        _consumer = consumer;
        _consumer.RequestReceivedAsync += OnRequestReceivedAsync;
    }

    /// <summary>
    /// 启动
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _producer.StartAsync(cancellationToken);
        await _consumer.StartAsync(cancellationToken);
    }

    /// <summary>
    /// 停止
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _producer.StopAsync(cancellationToken);
        _consumer.RequestReceivedAsync -= OnRequestReceivedAsync;
        await _consumer.StopAsync(cancellationToken);
    }

    /// <summary>
    /// 重试
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task RetryAsync(T request, CancellationToken cancellationToken = default) 
    {
        await _producer.PublishAsync(_consumer.KafkaOptions.Topic, request, cancellationToken);
    }

    /// <summary>
    /// 接收到消息，触发消息接收事件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task OnRequestReceivedAsync(T request)
    {
        await RequestReceivedAsync?.Invoke(request)!;
    }
}