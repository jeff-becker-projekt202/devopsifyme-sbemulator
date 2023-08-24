using Amqp.Listener;
using Microsoft.Extensions.Logging;
using ServiceBusEmulator.Abstractions.Options;
using ServiceBusEmulator.Abstractions.Security;
using ServiceBusEmulator.Security;
using System;
using System.Threading.Tasks;

namespace ServiceBusEmulator
{
    public class ServiceBusEmulatorHost
    {
        private bool _disposed;
        private IContainerHost _containerHost;
        private readonly Func<IContainerHost> _hostFactory;
        private readonly ILinkProcessor _linkProcessor;
        private readonly CbsRequestProcessor _cbsRequestProcessor;
        private readonly IServerCertificateFactory _certificateFactory;
        private readonly ILogger _logger;

        public ServiceBusEmulatorOptions Settings { get; }

        public ServiceBusEmulatorHost(
            Func<IContainerHost> hostFactory,
            ILinkProcessor linkProcessor, 
            CbsRequestProcessor cbsRequestProcessor, 
            IServerCertificateFactory certificateFactory, 
            ILogger<ServiceBusEmulatorHost> logger)
        {
            _hostFactory = hostFactory;
            _linkProcessor = linkProcessor;
            _cbsRequestProcessor = cbsRequestProcessor;
            _certificateFactory = certificateFactory;
            _logger = logger;
        }

        public async Task StartAsync()
        {
            try
            {
                _containerHost = _hostFactory();
                _logger.LogDebug($"Starting secure service bus host.");
                await Task.Run(() =>
                {
                    _containerHost.RegisterRequestProcessor("$cbs", _cbsRequestProcessor);
                    _containerHost.RegisterLinkProcessor(_linkProcessor);
                    _containerHost.Open();
                }).ConfigureAwait(false);
            }
            catch
            {
                _containerHost?.Close();
                _containerHost = null;
                throw;
            }
        }

        public Task StopAsync()
        {
            return Task.Run(Abort);
        }

        public void Abort()
        {
            try
            {
                StopHost();
            }
            finally
            {
                _containerHost = null;
            }
        }

        private void StopHost()
        {
            _containerHost.Close();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            Abort();
        }
    }
}
