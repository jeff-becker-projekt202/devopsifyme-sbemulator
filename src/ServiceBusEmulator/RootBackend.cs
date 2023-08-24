using Amqp.Handler;
using Amqp.Listener;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceBusEmulator.Abstractions;
using ServiceBusEmulator.Abstractions.Configuration;
using ServiceBusEmulator.Abstractions.Options;
using ServiceBusEmulator.Abstractions.Security;
using ServiceBusEmulator.Azure;
using ServiceBusEmulator.Security;
using System;
using System.Collections.Generic;

namespace ServiceBusEmulator;
public class RootBackend : IBackend
{

    public string Name => "Root";
    bool ShouldUse(IConfiguration configuration) => true;
    public void ApplyConfiguration(IWebAppBuilder builder)
    {
        _ = builder.Services.AddSingleton<AzureHandler>();
        _ = builder.Services.AddTransient<Func<ConnectionListener, IHandler>>(ctx => (ConnectionListener conn) => (IHandler)ctx.GetRequiredService<AzureHandler>());
        _ = builder.Services.AddSingleton(ServiceBusEmulatorContainerHost.CreateFactory);
        _ = builder.Services.AddTransient<ISecurityContext>(sp => SecurityContext.Default);
        _ = builder.Services.AddTransient<CbsRequestProcessor>();
        _ = builder.Services.AddTransient<ITokenValidator>(sp => CbsTokenValidator.Default);
        _ = builder.Services.AddOptions<ServiceBusEmulatorOptions>().BindConfiguration("Emulator");
        _ = builder.Services.AddTransient<ServiceBusEmulatorHost>();
        _ = builder.Services.AddSingleton<IHostedService, ServiceBusEmulatorWorker>();
        _ = builder.Services.AddSingleton(ctx => CertificateFactory.FromConfig(ctx.GetRequiredService<IConfiguration>()));
    }
    private readonly SwitchMapBuilder<ServiceBusEmulatorOptions> _swtichMap =
        SwitchMapBuilder<ServiceBusEmulatorOptions>.Create()
        .Add("emulator-host", x => x.HostName)
        .Add("emulator-port", x => x.Port)
        .Add("host", x => x.HostName)
        .Add("port", x => x.Port);
    public IMapSwitches SwitchMappings => _swtichMap.Mapper;
}
