namespace IKun.RabbitMQ.Messageing
{
    /// <summary>
    /// RabbitMQ生产者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RabbitMqProducer<T> : RabbitMqBase, IRabbitMqProducer<T> where T : IRabbitMqRequest
    {
        private readonly ILogger<RabbitMqProducer<T>> _logger;

        private IModel _publisherModel = null!;

        private readonly BlockingCollection<UnPublishRequest> _unPublishRequests = new BlockingCollection<UnPublishRequest>();

        /// <summary>
        /// RabbitMQ生产者
        /// </summary>
        /// <param name="connectionProxy"></param>
        /// <param name="logger"></param>
        public RabbitMqProducer(
            IRabbitMqConnectionProxy connectionProxy,
            ILogger<RabbitMqProducer<T>> logger)
            : base(
                connectionProxy,
                logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await Task.Run(() => Reconnect(cancellationToken), cancellationToken);
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task StopAsync(int milliseconds, CancellationToken cancellationToken = default)
        {
            ServiceStopped = true;

            try
            {
                ManualResetEvent.Set();
                _publisherModel.Close();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"[{RabbitMqOptions.HostName}]: RabbitMQ Publisher Stop Exception");
            }

            await base.StopAsync(milliseconds, cancellationToken);

            _logger.LogInformation($"[{RabbitMqOptions.HostName}]: RabbitMQ Publisher Stopped");
        }

        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey"></param>
        /// <param name="properties"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task PublishAsync(byte[] message, string exchangeName, string routingKey, IBasicProperties? properties = null, CancellationToken cancellationToken = default)
        {
            if (_publisherModel is { IsClosed: true })
            {
                _unPublishRequests.Add(new UnPublishRequest
                {
                    RequestBody = message,
                    RoutingKey = routingKey,
                    ExchangeName = exchangeName,
                    Timeout = DateTime.Now.AddSeconds(RabbitMqOptions.NetworkRecoveryInterval)
                }, cancellationToken);

                return;
            }

            try
            {
                _logger.LogInformation($"[{RabbitMqOptions.HostName}]: RabbitMQ Publisher Publish Message - Start");

                _publisherModel.BasicPublish(exchangeName, routingKey, properties, message);

                _logger.LogInformation($"[{RabbitMqOptions.HostName}]: RabbitMQ Publisher Publish Message - successfully! -> {routingKey}");
            }
            catch (Exception e)
            {
                _unPublishRequests.Add(new UnPublishRequest
                {
                    RequestBody = message,
                    RoutingKey = routingKey,
                    ExchangeName = exchangeName,
                    Timeout = DateTime.Now.AddSeconds(RabbitMqOptions.NetworkRecoveryInterval)
                }, cancellationToken);

                _logger.LogWarning(e, $"[{RabbitMqOptions.HostName}]: RabbitMQ Publisher Publish Message - Exception");
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey"></param>
        /// <param name="properties"></param>
        public void Publish(byte[] message, string exchangeName, string routingKey, IBasicProperties? properties = null)
        {
            if (_publisherModel is { IsClosed: true })
            {
                _unPublishRequests.Add(new UnPublishRequest
                {
                    RequestBody = message,
                    RoutingKey = routingKey,
                    ExchangeName = exchangeName,
                    Timeout = DateTime.Now.AddSeconds(RabbitMqOptions.NetworkRecoveryInterval)
                });

                return;
            }

            try
            {
                _logger.LogInformation($"[{RabbitMqOptions.HostName}]: RabbitMQ Publisher Publish Message - Start");

                _publisherModel.BasicPublish(exchangeName, routingKey, properties, message);

                _logger.LogInformation($"[{RabbitMqOptions.HostName}]: RabbitMQ Publisher Publish Message - successfully! -> {routingKey}");
            }
            catch (Exception e)
            {
                _unPublishRequests.Add(new UnPublishRequest
                {
                    RequestBody = message,
                    RoutingKey = routingKey,
                    ExchangeName = exchangeName,
                    Timeout = DateTime.Now.AddSeconds(RabbitMqOptions.NetworkRecoveryInterval)
                });

                _logger.LogWarning(e, $"[{RabbitMqOptions.HostName}]: RabbitMQ Publisher Publish Message - Exception");
            }
        }

        /// <summary>
        /// 连接就绪
        /// </summary>
        /// <param name="cancellationToken"></param>
        protected override void ConnectionReady(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"[{RabbitMqOptions.HostName}]: RabbitMQ Publisher CreateModel - Start");

            var channel = Connection.CreateModel();
            var oldChannel = _publisherModel;
            _publisherModel = channel;

            _logger.LogInformation($"[{RabbitMqOptions.HostName}]: RabbitMQ Publisher CreateModel - Finished");

            try
            {
                if (oldChannel is { IsClosed: false })
                {
                    oldChannel.Close();
                    oldChannel.Dispose();
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"[{RabbitMqOptions.HostName}]: RabbitMQ Publisher Dispose Old Channel Exception");
            }

            ProcessUnpublishRequests(cancellationToken);
        }

        /// <summary>
        /// 连接检查
        /// </summary>
        /// <param name="cancellationToken"></param>
        protected override void ConnectionCheck(CancellationToken cancellationToken = default)
        {
            Thread.CurrentThread.Name = $"{RabbitMqOptions.HostName}: RabbitMQ Publisher ConnectionCheck";

            _logger.LogInformation($"[{RabbitMqOptions.HostName}]: RabbitMQ Publisher ConnectionCheck - Start");

            while (!cancellationToken.IsCancellationRequested)
            {
                ManualResetEvent.WaitOne(RabbitMqOptions.NetworkRecoveryInterval * 1000);

                if (ServiceStopped)
                {
                    break;
                }

                if (_publisherModel is { IsClosed: false })
                {
                    continue;
                }

                _logger.LogInformation($"[{RabbitMqOptions.HostName}]: RabbitMQ Publisher ConnectionCheck - Raised");

                Reconnect(cancellationToken);
            }
        }

        /// <summary>
        /// 处理未发布的请求
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void ProcessUnpublishRequests(CancellationToken cancellationToken = default)
        {
            var publishCount = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (_publisherModel is { IsClosed: true })
                {
                    break;
                }

                if (!_unPublishRequests.TryTake(out var request))
                {
                    break;
                }

                if (DateTime.Now > request.Timeout)
                {
                    _logger.LogWarning($"{RabbitMqOptions.HostName}: RabbitMQ Publisher ProcessUnPublishRequest Request Timeout", "RabbitMqPublisher.ProcessUnPublishRequest");
                    continue;
                }

                try
                {
                    var routingKey = string.IsNullOrWhiteSpace(request.RoutingKey) ? RabbitMqOptions.RoutingKey : request.RoutingKey;
                    _publisherModel.BasicPublish(RabbitMqOptions.Exchange, routingKey, null, request.RequestBody);
                    publishCount++;
                }
                catch (Exception? e)
                {
                    _logger.LogWarning(e, $"{RabbitMqOptions.HostName}: RabbitMQ Publisher ProcessUnPublishRequests still Exception");
                }
            }

            if (publishCount > 0)
            {
                _logger.LogInformation($"{RabbitMqOptions.HostName}: RabbitMQ Publisher Process {publishCount} UnPublished Request(s) Successfully!");
            }
        }
    }
}