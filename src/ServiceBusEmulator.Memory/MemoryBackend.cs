using Amqp.Listener;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ServiceBusEmulator.Abstractions;
using ServiceBusEmulator.Abstractions.Configuration;
using ServiceBusEmulator.Abstractions.Options;
using ServiceBusEmulator.Memory.Entities;


namespace ServiceBusEmulator.Memory;
public class MemoryBackend : IBackend
{
    private const string ConfigSectionPath = "Emulator:Memory";

    public string Name => "Root";
    public void ApplyConfiguration(IWebAppBuilder builder)
    {
        _ = builder.Services.AddSingleton<ILinkProcessor, InMemoryLinkProcessor>();
        _ = builder.Services.AddSingleton<IEntityLookup, EntityLookup>();
        _ = builder.Services.AddSingleton<IHealthCheck, InMemoryHealthCheck>();
        _ = builder.Services.AddOptions<MemoryBackendOptions>().BindConfiguration(ConfigSectionPath);
        _ = builder.Services.AddHealthChecks().AddCheck<InMemoryHealthCheck>("in-memory");
    }
    private readonly SwitchMapBuilder<MemoryBackendOptions> _swtichMap =
        SwitchMapBuilder<MemoryBackendOptions>.Create(ConfigSectionPath)
        .Add("queue", x => x.Queues)
        .Add("topic", x => x.Topics)
        .Add("subscription", x => x.Subscriptions);
    public IMapSwitches SwitchMappings => _swtichMap.Mapper;

}
