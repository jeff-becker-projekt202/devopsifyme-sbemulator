
namespace ServiceBusEmulator.Abstractions.Configuration;

public class AggregateSwitchMapper : IMapSwitches
{
    private readonly ICollection<IMapSwitches> _switchMappers;

    public AggregateSwitchMapper(IEnumerable<IMapSwitches> switchMappers)
    {
        _switchMappers = switchMappers.ToArray();
    }
    public bool CanHandle(string arg)=>_switchMappers.Any(s=>s.CanHandle(arg));

    public string Transform(string arg)
    {
        var m = _switchMappers.FirstOrDefault(s => s.CanHandle(arg));
        return m != null? m.Transform(arg) : arg;
    }
}
