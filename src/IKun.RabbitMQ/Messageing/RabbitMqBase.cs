namespace IKun.RabbitMQ.Messageing
{
    /// <summary>
    /// RabbitMQ Base
    /// </summary>
    public abstract class RabbitMqBase
    {
        /// <summary>
        /// RabbitMQ Configuration
        /// </summary>
        public IRabbitMqOptions RabbitMqOptions { get; }

        /// <summary>
        /// Connection
        /// </summary>
        protected IConnection Connection = null!;

        /// <summary>
        /// Service Stopped
        /// </summary>
        protected bool ServiceStopped = false;

        /// <summary>
        /// Manual Reset Event
        /// </summary>
        protected ManualResetEvent ManualResetEvent = new ManualResetEvent(false);

        private readonly IRabbitMqConnectionProxy _connectionProxy;
        private readonly ILogger<RabbitMqBase> _logger;

        private bool _connectionCheckThreadCreated;

        /// <summary>
        /// RabbitMQ Base
        /// </summary>
        /// <param name="connectionProxy"></param>
        /// <param name="logger"></param>
        protected RabbitMqBase(
            IRabbitMqConnectionProxy connectionProxy,
            ILogger<RabbitMqBase> logger)
        {
            _connectionProxy = connectionProxy;
            _logger = logger;

            RabbitMqOptions = connectionProxy.RabbitMqOptions;

            CheckNullValue();
        }

        /// <summary>
        /// Stop
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task StopAsync(int milliseconds, CancellationToken cancellationToken = default)
        {
            try
            {
                Connection.Close(TimeSpan.FromMilliseconds(milliseconds));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"[{RabbitMqOptions.HostName}]: Error while closing connection");
            }

            _logger.LogInformation($"[{RabbitMqOptions.HostName}]: Connection closed");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Reconnect
        /// </summary>
        /// <param name="cancellationToken"></param>
        protected void Reconnect(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (ServiceStopped)
                    {
                        return;
                    }

                    InitConnection(cancellationToken);
                    SetupConnectionCheckThread(cancellationToken);

                    break;
                }
                catch (Exception? e)
                {
                    _logger.LogError(e, $"[{RabbitMqOptions.HostName}]: RabbitMQ Reconnect Failed!");
                    Thread.Sleep(RabbitMqOptions.RetryCreateDelayMilliseconds);
                }
            }
        }

        /// <summary>
        /// Init Connection
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void InitConnection(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"[{RabbitMqOptions.HostName}]: RabbitMQ create connection - Start");

            var newConnection = _connectionProxy.CreateConnection();
            var oldConnection = Connection;
            Connection = newConnection;

            _logger.LogInformation($"[{RabbitMqOptions.HostName}]: RabbitMQ create connection - Finished");

            try
            {
                if (oldConnection is { IsOpen: true })
                {
                    oldConnection.Close();
                    oldConnection.Dispose();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"[{RabbitMqOptions.HostName}]: Error while disposing old connection");
            }

            ConnectionReady(cancellationToken);
        }

        /// <summary>
        /// Setup Connection Check Thread
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void SetupConnectionCheckThread(CancellationToken cancellationToken = default)
        {
            if (_connectionCheckThreadCreated)
            {
                return;
            }

            var thread = new Thread(() => ConnectionCheck(cancellationToken))
            {
                IsBackground = true
            };

            thread.Start();

            _connectionCheckThreadCreated = true;
        }

        /// <summary>
        /// Connection Ready
        /// </summary>
        /// <param name="cancellationToken"></param>
        protected abstract void ConnectionReady(CancellationToken cancellationToken = default);

        /// <summary>
        /// Connection Check
        /// </summary>
        /// <param name="cancellationToken"></param>
        protected abstract void ConnectionCheck(CancellationToken cancellationToken = default);

        /// <summary>
        /// Check Null Value
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        private void CheckNullValue()
        {
            if (string.IsNullOrEmpty(RabbitMqOptions.HostName))
            {
                throw new ArgumentNullException(nameof(RabbitMqOptions.HostName));
            }

            if (string.IsNullOrEmpty(RabbitMqOptions.Exchange))
            {
                throw new ArgumentNullException(nameof(RabbitMqOptions.Exchange));
            }

            if (string.IsNullOrEmpty(RabbitMqOptions.QueueName))
            {
                throw new ArgumentNullException(nameof(RabbitMqOptions.QueueName));
            }

            if (RabbitMqOptions.NetworkRecoveryInterval == 0)
            {
                throw new ArgumentNullException(nameof(RabbitMqOptions.NetworkRecoveryInterval));
            }

            if (!RabbitMqOptions.UseSsl)
            {
                return;
            }

            if (string.IsNullOrEmpty(RabbitMqOptions.CertSubject))
            {
                throw new ArgumentNullException(nameof(RabbitMqOptions.CertSubject));
            }

            if (string.IsNullOrEmpty(RabbitMqOptions.CertficateThumbPrint))
            {
                throw new ArgumentNullException(nameof(RabbitMqOptions.CertficateThumbPrint));
            }
        }
    }
}