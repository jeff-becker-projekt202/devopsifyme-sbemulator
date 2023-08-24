using ServiceBusEmulator.Abstractions.Configuration;

namespace ServiceBusEmulator.Host;
public class GenericConfigurationSource : IConfigurationSource
{
    private readonly Func<IConfigurationBuilder, IConfigurationProvider> _build;
    public GenericConfigurationSource(Func<IConfigurationBuilder, IConfigurationProvider> _build)
    {
        this._build = _build;
    }
    public IConfigurationProvider Build(IConfigurationBuilder builder) => _build(builder);
}

public static class EmulatorHostCommandlineConfigurationExtensions
{

}
/// <summary>
/// A command line based <see cref="ConfigurationProvider"/>.
/// </summary>
public class EmulatorHostCommandLineConfigurationProvider : ConfigurationProvider
{
    private readonly IEnumerable<string> _args;
    private readonly IMapSwitches _switchMapper;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="args">The command line args.</param>
    /// <param name="switchMappings">The switch mappings.</param>
    public EmulatorHostCommandLineConfigurationProvider(IEnumerable<string> args, IMapSwitches switchMapper)
    {
        _args = args ?? throw new ArgumentNullException(nameof(args));
        _switchMapper = switchMapper ?? throw new ArgumentNullException(nameof(switchMapper));

    }



    /// <summary>
    /// Loads the configuration data from the command line args.
    /// </summary>
    public override void Load()
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        string key, value;

        using (IEnumerator<string> enumerator = _args.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                string currentArg = enumerator.Current;
                int keyStartIndex = 0;

                if (currentArg.StartsWith("--"))
                {
                    keyStartIndex = 2;
                }
                else if (currentArg.StartsWith("-"))
                {
                    keyStartIndex = 1;
                }
                else if (currentArg.StartsWith("/"))
                {
                    // "/SomeSwitch" is equivalent to "--SomeSwitch" when interpreting switch mappings
                    // So we do a conversion to simplify later processing
                    currentArg = $"--{currentArg.Substring(1)}";
                    keyStartIndex = 2;
                }

                int separator = currentArg.IndexOf('=');

                if (separator < 0)
                {
                    // If there is neither equal sign nor prefix in current argument, it is an invalid format
                    if (keyStartIndex == 0)
                    {
                        // Ignore invalid formats
                        continue;
                    }

                    // If the switch is a key in given switch mappings, interpret it
                    if ( _switchMapper.CanHandle(currentArg))
                    {
                        key = _switchMapper.Transform(currentArg);
                    }
                    // If the switch starts with a single "-" and it isn't in given mappings , it is an invalid usage so ignore it
                    else if (keyStartIndex == 1)
                    {
                        continue;
                    }
                    // Otherwise, use the switch name directly as a key
                    else
                    {
                        key = currentArg.Substring(keyStartIndex);
                    }

                    if (!enumerator.MoveNext())
                    {
                        // ignore missing values
                        continue;
                    }

                    value = enumerator.Current;
                }
                else
                {
                    string keySegment = currentArg.Substring(0, separator);

                    // If the switch is a key in given switch mappings, interpret it
                    if (_switchMapper.CanHandle(keySegment) )
                    {
                        key = _switchMapper.Transform(keySegment);
                    }
                    // If the switch starts with a single "-" and it isn't in given mappings , it is an invalid usage
                    else if (keyStartIndex == 1)
                    {
                        throw new FormatException($"The short-switch {currentArg} is not defined.");
                    }
                    // Otherwise, use the switch name directly as a key
                    else
                    {
                        key = currentArg.Substring(keyStartIndex, separator - keyStartIndex);
                    }

                    value = currentArg.Substring(separator + 1);
                }

                // Override value when key is duplicated. So we always have the last argument win.
                data[key] = value;
            }
        }

        Data = data;
    }

}