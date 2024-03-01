namespace IKun.RabbitMQ.Messageing
{
    /// <summary>
    /// RabbitMQ Connection Proxy
    /// </summary>
    public class RabbitMqConnectionProxy : IRabbitMqConnectionProxy
    {
        /// <summary>
        /// RabbitMQ Configuration
        /// </summary>
        public IRabbitMqOptions RabbitMqOptions { get; }

        private readonly ILogger<RabbitMqConnectionProxy> _logger;

        private ConnectionFactory _connectionFactory = null!;

        private string _certificateThumbprint = null!;
        private string _certSubjectName = null!;

        private static readonly object CertStorySyncRoot = new object();

        /// <summary>
        /// RabbitMQ Connection Proxy
        /// </summary>
        /// <param name="rabbitMqOptions"></param>
        /// <param name="logger"></param>
        public RabbitMqConnectionProxy(
            IRabbitMqOptions rabbitMqOptions,
            ILogger<RabbitMqConnectionProxy> logger)
        {
            RabbitMqOptions = rabbitMqOptions;
            _logger = logger;

            InitRabbitMqConnectionFactory();
        }

        /// <summary>
        /// 创建连接
        /// </summary>
        /// <returns></returns>
        public IConnection CreateConnection()
        {
            return _connectionFactory.CreateConnection();
        }

        /// <summary>
        /// 初始化RabbitMQ连接工厂
        /// </summary>
        private void InitRabbitMqConnectionFactory()
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = RabbitMqOptions.HostName,
                Port = RabbitMqOptions.Port,
                VirtualHost = RabbitMqOptions.VirtualHost,
                DispatchConsumersAsync = true
            };

            if (RabbitMqOptions.UseSsl)
            {
                _connectionFactory.RequestedHeartbeat = TimeSpan.FromSeconds(RabbitMqOptions.RequestedHeartbeat);
                _connectionFactory.AutomaticRecoveryEnabled = RabbitMqOptions.AutomaticRecoveryEnabled;
                _connectionFactory.TopologyRecoveryEnabled = RabbitMqOptions.TopologyRecoveryEnabled;
                _connectionFactory.NetworkRecoveryInterval = TimeSpan.FromSeconds(RabbitMqOptions.NetworkRecoveryInterval);
                _connectionFactory.Port = RabbitMqOptions.Port;
                _connectionFactory.Ssl = new SslOption
                {
                    Enabled = RabbitMqOptions.UseSsl,
                    ServerName = RabbitMqOptions.HostName,
                    Version = (SslProtocols)ServicePointManager.SecurityProtocol,
                    //由于证书是自签名的
                    AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch |
                                             SslPolicyErrors.RemoteCertificateChainErrors
                };
                _connectionFactory.Ssl.CertificateSelectionCallback += CertificateSelectionCallback;

                _connectionFactory.AuthMechanisms = new List<IAuthMechanismFactory>
            {
                new ExternalMechanismFactory()
                };

                if (!string.IsNullOrWhiteSpace(RabbitMqOptions.CertSubject))
                {
                    _certSubjectName = RabbitMqOptions.CertSubject;
                }

                if (!string.IsNullOrWhiteSpace(RabbitMqOptions.CertficateThumbPrint))
                {
                    _certificateThumbprint = Regex.Replace(RabbitMqOptions.CertficateThumbPrint, @"[^\da-fA-F]", string.Empty).ToLower();
                }
            }
            else
            {
                _connectionFactory.UserName = RabbitMqOptions.UserName;
                _connectionFactory.Password = RabbitMqOptions.Password;
            }
        }

        /// <summary>
        /// Certificate Selection Callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="targetHost"></param>
        /// <param name="localCertficate"></param>
        /// <param name="remoteCertificate"></param>
        /// <param name="acceptableIssuers"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private X509Certificate CertificateSelectionCallback(
            object sender,
            string targetHost,
            X509CertificateCollection localCertficate,
            X509Certificate? remoteCertificate,
            string[] acceptableIssuers)
        {
            lock (CertStorySyncRoot)
            {
                using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);

                _logger.LogInformation($"{RabbitMqOptions.HostName}: CertificateSelectionCallback - Start -> Certificate");

                store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection? certificates = null;

                if (!string.IsNullOrWhiteSpace(_certSubjectName))
                {
                    certificates = store.Certificates.Find(X509FindType.FindBySubjectName, _certSubjectName, true);
                }

                if (certificates == null || certificates.Count <= 0)
                {
                    _logger.LogError($"CertificateName: '{_certSubjectName}', CertificateSelectionCallback - Error (certificates is null) -> CertificateThumbprint - {_certificateThumbprint}; CertSubject - {_certSubjectName}");

                    throw new Exception($"CertificateName: '{_certSubjectName}', CertificateThumbprint: '{_certificateThumbprint}' not found in store LocalMachine/My.");
                }

                _logger.LogInformation($"{RabbitMqOptions.HostName}: CertificateSelectionCallback - Finished -> -> CertificateThumbprint - {_certificateThumbprint}; CertSubject - {_certSubjectName}");

                return certificates[0];
            }
        }
    }
}