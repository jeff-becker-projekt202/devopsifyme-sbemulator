using Amqp.Listener;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceBusEmulator.Abstractions.Options;
using ServiceBusEmulator.Abstractions.Security;
using ServiceBusEmulator.Security;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace ServiceBusEmulator
{
    public static class Extensions
    {
        public static IServiceCollection AddServiceBusEmulator(this IServiceCollection services, Action<ServiceBusEmulatorOptions> configure = null)
        {
            configure ??= (o) => { };


            _ = services.AddTransient<ISecurityContext>(sp => SecurityContext.Default);

            _ = services.AddTransient<CbsRequestProcessor>();
            _ = services.AddTransient<ITokenValidator>(sp => CbsTokenValidator.Default);

            _ = services.AddOptions<ServiceBusEmulatorOptions>().Configure(configure).PostConfigure<IServerCertificateFactory>((options, certFactory) =>
            {
                options.CertificateFactory = certFactory;
                //if (!string.IsNullOrEmpty(options.ServerCertificateThumbprint))
                //{
                //    using X509Store store = new(StoreName.My, StoreLocation.CurrentUser);
                //    store.Open(OpenFlags.ReadOnly);
                //    options.ServerCertificate = store.Certificates.Find(X509FindType.FindByThumbprint, options.ServerCertificateThumbprint, false).FirstOrDefault();
                //}

                //if (!string.IsNullOrEmpty(options.ServerCertificatePath))
                //{
                //    options.ServerCertificate = new X509Certificate2(options.ServerCertificatePath, options.ServerCertificatePassword, X509KeyStorageFlags.Exportable);
                //}
            }).BindConfiguration("Emulator"); ;

            _ = services.AddTransient<ServiceBusEmulatorHost>();
            _ = services.AddSingleton<IHostedService, ServiceBusEmulatorWorker>();

            return services;
        }
    }
}
