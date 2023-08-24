using Microsoft.Extensions.Configuration.CommandLine;
using ServiceBusEmulator.Abstractions.Configuration;

namespace ServiceBusEmulator.Host;

public static class CommandLineHelper
{
    public static IConfigurationBuilder AddBackendCommandLineSwitch(this IConfigurationBuilder builder, IEnumerable<string> args)
    {
        builder.Add(new GenericConfigurationSource((b)=> new BackendSwitchCommandLineProvider(args)));
        return builder;
    }
    public static IConfigurationBuilder AddEmulatorHostCommandline(this IConfigurationBuilder builder, IEnumerable<string> args, IMapSwitches switchMapper)
    {
        builder.Add(new GenericConfigurationSource((b) => new EmulatorHostCommandLineConfigurationProvider(args, switchMapper)));
        return builder;
    }
    public class BackendSwitchCommandLineProvider : CommandLineConfigurationProvider
    {
        public const string BackendConfigKey = "Emulator:Backend";
        public BackendSwitchCommandLineProvider(IEnumerable<string> args) : base(args, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
         {"--backend",BackendConfigKey}
    })
        {
        }
        public override void Load()
        {
            base.Load();
            Data = Data.Where(d => d.Key == BackendConfigKey).ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        }
    }
}


