using Amqp;
using Amqp.Framing;
using Amqp.Handler;
using Amqp.Listener;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceBusEmulator.Abstractions.Options;
using ServiceBusEmulator.Abstractions.Security;
using ServiceBusEmulator.Azure;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace ServiceBusEmulator
{
    public class ServiceBusEmulatorContainerHost : ContainerHost, IContainer
    {
        private readonly ILogger<ServiceBusEmulatorContainerHost> _logger;

        public ServiceBusEmulatorContainerHost(ILogger<ServiceBusEmulatorContainerHost> logger, Func<ConnectionListener, IHandler> handlerFactory, IList<Address> addressList, X509Certificate2 certificate) : base(addressList, certificate)
        {
            AddressResolver += (c, attach) =>
            {
                // required for node.js SDK $cbs authentication
                ((Target)attach.Target).Address ??= attach.LinkName;
                return null;
            };
            _logger = logger;
            Listeners[0].HandlerFactory = handlerFactory;
            var profile = new AzureSaslProfile();
            Listeners[0].SASL.EnableMechanism(profile.Mechanism, profile);
            Listeners[0].SASL.EnableExternalMechanism = true;
            Listeners[0].SASL.EnableAnonymousMechanism = false;
            Listeners[0].SSL.ClientCertificateRequired = false;
            Listeners[0].SSL.Certificate = certificate;
            Listeners[0].SSL.RemoteCertificateValidationCallback = RemoteCertificateValidationCallback;

        }
        private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return errors == System.Net.Security.SslPolicyErrors.RemoteCertificateNotAvailable;
        }
        public static Func<IContainerHost> CreateFactory(IServiceProvider ctx)
        {
            return () =>
            {
                var Settings = ctx.GetRequiredService<IOptions<ServiceBusEmulatorOptions>>().Value;
                var address = new Address($"amqps://{Settings.HostName}:{Settings.Port}");
                var certificateFactory = ctx.GetRequiredService<IServerCertificateFactory>();
                var logger = ctx.GetRequiredService<ILoggerFactory>().CreateLogger<ServiceBusEmulatorContainerHost>();
                var certificate = certificateFactory.Load();
                var handlerFactory = ctx.GetRequiredService<Func<ConnectionListener, IHandler>>();
                var host = new ServiceBusEmulatorContainerHost(logger, handlerFactory, new[] { address }, certificate);
                return host;
            };
        }


    }
}
