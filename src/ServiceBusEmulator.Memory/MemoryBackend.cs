﻿using Amqp.Listener;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ServiceBusEmulator.Abstractions;
using ServiceBusEmulator.Abstractions.Configuration;
using ServiceBusEmulator.Abstractions.Options;



namespace ServiceBusEmulator.Memory;
public class MemoryBackend : IBackend
{
    private const string ConfigSectionPath = "Emulator:Memory";

    public string Name => "Memory";
    public void ApplyConfiguration(IWebAppBuilder builder)
    {
        _ = builder.Services.AddSingleton<ILinkProcessor, InMemoryLinkProcessor>();
        //_ = builder.Services.AddSingleton<IEntityLookup, EntityLookup>();
        _ = builder.Services.AddSingleton<ChannelMap>();
        _ = builder.Services.AddSingleton<IHealthCheck, InMemoryHealthCheck>();
        _ = builder.Services.AddOptions<MemoryBackendOptions>().BindConfiguration(ConfigSectionPath);
        _ = builder.Services.AddHealthChecks().AddCheck<InMemoryHealthCheck>("in-memory");
        //_ = builder.Services.AddSingleton<IHostedService, MemoryWorker>();
    }
    private readonly SwitchMapBuilder<MemoryBackendOptions> _swtichMap =
        SwitchMapBuilder<MemoryBackendOptions>.Create(ConfigSectionPath)
            .Add("memory-queue", x => x.Queues)
            .Add("memory-topic", x => x.Topics)
            .Add("memory-subscription", x => x.Subscriptions);
    public IMapSwitches SwitchMappings => _swtichMap.Mapper;
    public bool ShouldUse(IConfiguration configuration) => String.Compare(configuration.GetSection("Emulator:Backend").Value ?? Name, Name, true) == 0;

}
