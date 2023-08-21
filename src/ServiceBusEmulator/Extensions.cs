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
            _ = services.AddOptions<ServiceBusEmulatorOptions>().Configure(configure).BindConfiguration("Emulator");
            _ = services.AddTransient<ServiceBusEmulatorHost>();
            _ = services.AddSingleton<IHostedService, ServiceBusEmulatorWorker>();

            return services;
        }
    }
}
