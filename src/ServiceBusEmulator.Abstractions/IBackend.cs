using Microsoft.Extensions.Configuration;
using ServiceBusEmulator.Abstractions.Configuration;
using ServiceBusEmulator.Abstractions.Options;
using System.Collections;

namespace ServiceBusEmulator.Abstractions;

public interface IBackend
{
    string Name { get; }
    void ApplyConfiguration(IWebAppBuilder builder);
    bool ShouldUse(IConfiguration configuration) =>String.Compare(configuration.GetSection("Emulator:Backend").Value?? "", Name, true) == 0;
    IMapSwitches SwitchMapper => new DefaultSwitchMapper();
    
}
