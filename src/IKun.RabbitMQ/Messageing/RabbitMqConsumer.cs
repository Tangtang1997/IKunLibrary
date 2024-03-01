namespace IKun.RabbitMQ.Messageing
{
    /// <summary>
    /// RabbitMQ Consumer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RabbitMqConsumer<T> : RabbitMqBase, IRabbitMqConsumer<T> where T : IRabbitMqRequest
    {
        /// <summary>
        /// RabbitMQ Configuration
        /// </summary>
        public event Func<byte[], Task> RequestReceivedAsync = null!;

        private readonly ILogger<RabbitMqConsumer<T>> _logger;

        private IModel _consumerModel = null!;
        private AsyncEventingBasicConsumer _asyncConsumer = null!;

        private readonly CountdownEvent _countdownEvent = new CountdownEvent(1);

        private readonly BlockingCollection<UnAckRequest> _unAckQueue = new BlockingCollection<UnAckRequest>();
        private const int AckTimeoutSeconds = 180;

        /// <summary>
        /// RabbitMQ Consumer
        /// </summary>
        /// <param name="connectionProxy"></param>
        /// <param name="logger"></param>
        public RabbitMqConsumer(
            IRabbitMqConnectionProxy connectionProxy,
            ILogger<RabbitMqConsumer<T>> logger)
            : base(
                connectionProxy,
                logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await Task.Run(() => Reconnect(cancellationToken), cancellationToken);
        }

        /// <summary>
        /// Stop
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task StopAsync(int milliseconds, CancellationToken cancellationToken = default)
        {
            ServiceStopped = true;
            try
            {
                ManualResetEvent.Set();
                _asyncConsumer.Received -= ConsumerReceivedAsync;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"[{RabbitMqOptions.HostName}]: RabbitMQ Consumer Stopped Error");
            }

            return base.StopAsync(milliseconds, cancellationToken);
        }

        /// <summary>
        /// Connection Ready
        /// </summary>
        /// <param name="cancellationToken"></param>
        protected override void ConnectionReady(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"[{RabbitMqOptions.HostName}]: RabbitMQ consumer CreateModel - Start");

            var consumerModel = Connection.CreateModel();

            var consumerQueueArgument = new Dictionary<string, object>();

            try
            {
                if (!string.IsNullOrEmpty(RabbitMqOptions.DeadLetterExchange) &&
                    !string.IsNullOrEmpty(RabbitMqOptions.DeadLetterRoutingKey) &&
                    !string.IsNullOrEmpty(RabbitMqOptions.DeadLetterQueueName))
                {
                    consumerModel.ExchangeDeclare(
                        exchange: RabbitMqOptions.DeadLetterExchange,
                        durable: RabbitMqOptions.Durable,
                        type: RabbitMqOptions.ExchangeType,
                        autoDelete: false
                        );

                    consumerModel.QueueDeclare(
                        queue: RabbitMqOptions.DeadLetterQueueName,
                        durable: RabbitMqOptions.Durable,
                        exclusive: false,
                        autoDelete: false
                        );

                    consumerModel.QueueBind(
                        queue: RabbitMqOptions.DeadLetterQueueName,
                        exchange: RabbitMqOptions.DeadLetterExchange,
                        routingKey: RabbitMqOptions.DeadLetterRoutingKey
                        );

                    consumerQueueArgument = new Dictionary<string, object>
                {
                    { Headers.XDeadLetterExchange, RabbitMqOptions.DeadLetterExchange },
                    { Headers.XMessageTTL, RabbitMqOptions.QueueTtl },
                    { Headers.XDeadLetterRoutingKey, RabbitMqOptions.DeadLetterRoutingKey }
                };
                }
                else
                {
                    _logger.LogWarning($"[{RabbitMqOptions.HostName}]: Dead letter exchange not exist");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"[{RabbitMqOptions.HostName}]: Error while creating dead letter exchange");
            }

            try
            {
                consumerModel.ExchangeDeclare(
                    exchange: RabbitMqOptions.Exchange,
                    durable: RabbitMqOptions.Durable,
                    type: RabbitMqOptions.ExchangeType,
                    autoDelete: false
                    );

                consumerModel.QueueDeclare(
                    queue: RabbitMqOptions.QueueName,
                    durable: RabbitMqOptions.Durable,
                    exclusive: false,
                    autoDelete: false,
                    arguments: consumerQueueArgument
                );

                consumerModel.QueueBind(
                    queue: RabbitMqOptions.QueueName,
                    exchange: RabbitMqOptions.Exchange,
                    routingKey: RabbitMqOptions.RoutingKey
                    );
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"[{RabbitMqOptions.HostName}]: Error while creating queue");
                throw;
            }

            var oldConsumerModel = _consumerModel;
            _consumerModel = consumerModel;

            _asyncConsumer = new AsyncEventingBasicConsumer(_consumerModel);
            _asyncConsumer.Received -= ConsumerReceivedAsync;
            _asyncConsumer.Received += ConsumerReceivedAsync;

            var consumeTag = _consumerModel.BasicConsume(
                queue: RabbitMqOptions.QueueName,
                autoAck: false,
                consumerTag: Environment.MachineName,
                consumer: _asyncConsumer
                );
            _asyncConsumer.HandleBasicCancelOk(consumeTag);

            _logger.LogInformation($"[{RabbitMqOptions.HostName}]: RabbitMQ consumer CreateModel - Finished");

            try
            {
                if (oldConsumerModel is { IsClosed: false })
                {
                    oldConsumerModel.Close();
                    oldConsumerModel.Dispose();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"[{RabbitMqOptions.HostName}]: Error while disposing old consumer model");
            }

            ProcessUnAckRequests(cancellationToken);
        }

        /// <summary>
        /// Connection Check
        /// </summary>
        /// <param name="cancellationToken"></param>
        protected override void ConnectionCheck(CancellationToken cancellationToken = default)
        {
            Thread.CurrentThread.Name = "RabbitMQ-Consumer-Thread";

            _logger.LogInformation($"[{RabbitMqOptions.HostName}]: RabbitMQ Consumer ConnectionCheck Start");
            while (!cancellationToken.IsCancellationRequested)
            {
                ManualResetEvent.WaitOne(RabbitMqOptions.NetworkRecoveryInterval * 1000);

                if (ServiceStopped)
                {
                    break;
                }

                if (_asyncConsumer is { IsRunning: true })
                {
                    continue;
                }

                _logger.LogInformation($"[{RabbitMqOptions.HostName}] : RabbitMQ Consumer Reconnect Raised");

                Reconnect(cancellationToken);
            }
        }

        /// <summary>
        /// Consumer Received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        private async Task ConsumerReceivedAsync(object? sender, BasicDeliverEventArgs eventArgs)
        {
            _countdownEvent.AddCount();
            await Task.Run(async () =>
            {
                await OnReceiveCompletedAsync(eventArgs);
                _countdownEvent.Signal();
            });
        }

        /// <summary>
        /// On Receive Completed
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task OnReceiveCompletedAsync(BasicDeliverEventArgs args)
        {
            var isFired = false;

            try
            {
                isFired = await FireReceiveEventAsync(args.Body.ToArray());

                if (isFired)
                {
                    _consumerModel.BasicAck(args.DeliveryTag, false);
                }
                else
                {
                    _consumerModel.BasicNack(args.DeliveryTag, false, false);
                }
            }
            catch (Exception e)
            {
                _unAckQueue.Add(new UnAckRequest
                {
                    DeliveryTag = args.DeliveryTag,
                    IsFired = isFired,
                    Timeout = DateTime.Now.AddSeconds(AckTimeoutSeconds)
                });

                _logger.LogError(e, $"[{RabbitMqOptions.HostName}]: RabbitMQ Consumer Ack Exception. Move it to quere and try later!");
            }
        }

        /// <summary>
        /// Fire Receive Event
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        private async Task<bool> FireReceiveEventAsync(byte[] body)
        {
            try
            {
                await RequestReceivedAsync.Invoke(body);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"[{RabbitMqOptions.HostName}]: RabbitMQ Consumer FireReceiveEvent Exception");
                return false;
            }
        }

        /// <summary>
        /// Process UnAck Requests
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void ProcessUnAckRequests(CancellationToken cancellationToken = default)
        {
            var ackCount = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (_consumerModel.IsClosed)
                {
                    break;
                }

                if (!_unAckQueue.TryTake(out var request))
                {
                    break;
                }

                if (DateTime.Now > request.Timeout)
                {
                    _logger.LogWarning($"{RabbitMqOptions.HostName}: RabbitMQ Consumer ProcessUnAckRequests Request Timeout");

                    continue;
                }

                try
                {
                    if (request.IsFired)
                    {
                        _consumerModel.BasicAck(request.DeliveryTag, false);
                    }
                    else
                    {
                        _consumerModel.BasicNack(request.DeliveryTag, false, false);
                    }

                    ackCount++;
                }
                catch (Exception? e)
                {
                    _logger.LogError(e, $"{RabbitMqOptions.HostName}: RabbitMQ Consumer ProcessUnAckRequests still Exception", "RabbitMqCOnsumer.ProcessUnAckRequest");
                }

                if (ackCount > 0)
                {
                    _logger.LogInformation($"{RabbitMqOptions.HostName}: RabbitMQ Consumer Processed {ackCount} UnAcked Request(s) Sucessfully!");
                }
            }
        }
    }
}