using ServiceBusEmulator.Abstractions;
using ServiceBusEmulator.Abstractions.Configuration;
using ServiceBusEmulator.Memory;
using ServiceBusEmulator.RabbitMq;

namespace ServiceBusEmulator.Host;

public class Backends
{
    public static readonly IReadOnlyList<IBackend> All = new IBackend[] { new RootBackend(), new RabbitMqBackend(), new MemoryBackend() };
    public static AggregateSwitchMapper SwitchMapper => new AggregateSwitchMapper(All.Select(b => b.SwitchMapper));

    public static IMapSwitches CreateSwitchMapper(IConfiguration configuration)
    {
        List<IBackend> inUse = GetInuseBackends(configuration);
        return new AggregateSwitchMapper(inUse.Select(b => b.SwitchMapper));
    }

    public static List<IBackend> GetInuseBackends(IConfiguration configuration)
    {
        var inUse = new List<IBackend>();
        foreach (var backend in All)
        {
            if (backend.ShouldUse(configuration))
            {
                inUse.Add(backend);
            }
        }

        return inUse;
    }
}
