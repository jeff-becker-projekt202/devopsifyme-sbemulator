using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceBusEmulator.Abstractions.Options;
using ServiceBusEmulator.Abstractions.Security;
using ServiceBusEmulator.Security;
using System;
using System.Linq;

namespace ServiceBusEmulator
{
    public static class Extensions
    {
        public static IWebAppBuilder AddServiceBusEmulator(this IWebAppBuilder builder, Action<ServiceBusEmulatorOptions> configure = null)
        {
            configure ??= (o) => { };
            _ = builder.Services.AddTransient<ISecurityContext>(sp => SecurityContext.Default);
            _ = builder.Services.AddTransient<CbsRequestProcessor>();
            _ = builder.Services.AddTransient<ITokenValidator>(sp => CbsTokenValidator.Default);
            _ = builder.Services.AddOptions<ServiceBusEmulatorOptions>().Configure(configure).BindConfiguration("Emulator");
            _ = builder.Services.AddTransient<ServiceBusEmulatorHost>();
            _ = builder.Services.AddSingleton<IHostedService, ServiceBusEmulatorWorker>();
            _ = builder.Services.AddSingleton(ctx => CertificateFactory.FromConfig(ctx.GetRequiredService<IConfiguration>()));

            return builder;
        }
    }
}
